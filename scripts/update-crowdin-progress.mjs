import { readFileSync, writeFileSync, readdirSync } from "node:fs";
import { join } from "node:path";
import { tmpdir } from "node:os";

const COMMIT_MESSAGE_PATH = join(tmpdir(), "crowdin-commit-message.txt");

const PROJECT_ID = process.env.CROWDIN_PROJECT_ID;
const TOKEN = process.env.CROWDIN_PERSONAL_TOKEN;
const SKIP_DIRS = new Set(["node_modules", ".git", ".vs", "bin", "obj"]);
const BULLET_RE = /^(-\s(?:⚠️|✔️|❌))([^0-9%>]+?)(\d+)%(.*)$/;
// Indent varies per file (tab here, 2 spaces elsewhere) - matched instead of hardcoded so a reformatted file doesn't silently break the insertion.
const CHANGELOG_CLOSE_RE = /^([ \t]*)<\/ChangeLog>$/m;
const LOCALISATION_HEADING_RE = /^### Localisation\s*$/m;
const HEADING_RE = /^###\s/;

// Walks from cwd looking for Properties/PublishConfiguration.xml, wherever it lives (repo root, src/, ...).
function findPublishConfig(dir) {
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    const full = join(dir, entry.name);
    if (entry.isDirectory()) {
      if (SKIP_DIRS.has(entry.name)) continue;
      const found = findPublishConfig(full);
      if (found) return found;
    } else if (entry.isFile() && entry.name === "PublishConfiguration.xml") {
      return full;
    }
  }
  return null;
}

function iconFor(percent) {
  if (percent >= 100) return "✔️";
  if (percent <= 0) return "❌";
  return "⚠️";
}

async function fetchProgress() {
  const res = await fetch(
    `https://api.crowdin.com/api/v2/projects/${PROJECT_ID}/languages/progress?limit=500`,
    { headers: { Authorization: `Bearer ${TOKEN}` } },
  );
  if (!res.ok) {
    throw new Error(`Crowdin API error ${res.status}: ${await res.text()}`);
  }
  const json = await res.json();
  return json.data.map(({ data }) => ({
    name: data.language.name,
    percent: data.translationProgress,
  }));
}

function updateLines(content, progress) {
  const changes = [];
  const unchanged = [];
  const lines = content.split("\n").map((line) => {
    const match = line.match(BULLET_RE);
    if (!match) return line;

    const [, prefix, namePart, oldPercentStr, rest] = match;
    const name = namePart.trim();
    const found = progress.find((p) => p.name === name);
    if (!found) return line;

    const oldPercent = Number(oldPercentStr);
    const newPercent = found.percent;
    if (newPercent === oldPercent) {
      unchanged.push({ name, percent: oldPercent });
      return line;
    }

    changes.push({ name, oldPercent, newPercent });
    return `- ${iconFor(newPercent)}${namePart}${newPercent}%${rest}`;
  });
  return { content: lines.join("\n"), changes, unchanged };
}

const buildBulletLine = (change) => `* ${change.name} : ${change.oldPercent}% → ${change.newPercent}%`;

// "Current" = languages tracked in the file that this commit leaves untouched (already reflected from an
// earlier run in the PR); "Changed" = languages this commit actually moves.
function buildCommitMessage(changes, unchanged) {
  const sections = [];
  if (unchanged.length > 0) {
    const bullets = unchanged.map((u) => `* ${u.name} : ${u.percent}%`).join("\n");
    sections.push(["Current", bullets].join("\n"));
  }
  if (changes.length > 0) {
    sections.push(["Changed", changes.map(buildBulletLine).join("\n")].join("\n"));
  }

  return ["Update translation progress from Crowdin", "", sections.join("\n\n")].join("\n");
}

// Updates bullets already in the section by language name (in place) instead of duplicating them; anything else in
// the section (manual notes, credit-only bullets for a language with no % change this run) is left untouched.
function mergeLocalisationSection(sectionLines, changes) {
  const remaining = new Map(changes.map((c) => [c.name, c]));
  const updatedLines = sectionLines.map((line) => {
    const m = line.match(/^\*\s*([^:]+?)\s*:/);
    if (!m) return line;
    const change = remaining.get(m[1].trim());
    if (!change) return line;
    remaining.delete(m[1].trim());
    return buildBulletLine(change);
  });
  for (const change of remaining.values()) updatedLines.push(buildBulletLine(change));
  return updatedLines;
}

function appendChangelogSection(content, changes) {
  if (changes.length === 0) return content;

  const closeMatch = content.match(CHANGELOG_CLOSE_RE);
  if (!closeMatch) return content;

  const headingMatch = content.match(LOCALISATION_HEADING_RE);
  if (!headingMatch) {
    const bullets = changes.map(buildBulletLine).join("\n");
    const section = `\n### Localisation\n${bullets}\n`;
    return content.replace(closeMatch[0], `${section}${closeMatch[0]}`);
  }

  // A "### Localisation" section already exists (from an earlier run) - update it in place instead of adding a duplicate heading.
  const lines = content.split("\n");
  const headingIndex = lines.findIndex((line) => LOCALISATION_HEADING_RE.test(line));
  let endIndex = lines.length;
  for (let i = headingIndex + 1; i < lines.length; i++) {
    if (HEADING_RE.test(lines[i]) || CHANGELOG_CLOSE_RE.test(lines[i])) {
      endIndex = i;
      break;
    }
  }
  const sectionLines = lines.slice(headingIndex + 1, endIndex);
  lines.splice(headingIndex + 1, sectionLines.length, ...mergeLocalisationSection(sectionLines, changes));
  return lines.join("\n");
}

if (!PROJECT_ID || !TOKEN) {
  console.error("Missing CROWDIN_PROJECT_ID or CROWDIN_PERSONAL_TOKEN env vars.");
  process.exit(1);
}

const filePath = findPublishConfig(process.cwd());
if (!filePath) {
  console.error("Could not find a PublishConfiguration.xml under the working directory.");
  process.exit(1);
}
console.log(`Using ${filePath}`);

const raw = readFileSync(filePath, "utf8");
// Normalize to LF for matching/editing (CRLF leaves a trailing \r that breaks the line-anchored regexes above),
// then restore the file's original line ending on write.
const usesCRLF = raw.includes("\r\n");
const normalized = raw.replace(/\r\n/g, "\n");

const progress = await fetchProgress();
const { content: withProgress, changes, unchanged } = updateLines(normalized, progress);

if (changes.length === 0) {
  console.log("No translation progress changes.");
  process.exit(0);
}

const final = appendChangelogSection(withProgress, changes);
writeFileSync(filePath, usesCRLF ? final.replace(/\n/g, "\r\n") : final);
writeFileSync(COMMIT_MESSAGE_PATH, buildCommitMessage(changes, unchanged));

console.log(`Updated ${changes.length} language(s):`);
for (const c of changes) {
  console.log(`  - ${c.name}: ${c.oldPercent}% -> ${c.newPercent}%`);
}
