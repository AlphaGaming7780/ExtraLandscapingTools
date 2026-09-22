import { readFileSync, writeFileSync } from "node:fs";

const PROJECT_ID = process.env.CROWDIN_PROJECT_ID;
const TOKEN = process.env.CROWDIN_PERSONAL_TOKEN;
const FILE_PATH = "Properties/PublishConfiguration.xml";
const CHANGELOG_CLOSE_TAG = "  </ChangeLog>";
const BULLET_RE = /^(-\s(?:⚠️|✔️|❌))([^0-9%>]+?)(\d+)%(.*)$/;

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
  const lines = content.split("\n").map((line) => {
    const match = line.match(BULLET_RE);
    if (!match) return line;

    const [, prefix, namePart, oldPercentStr, rest] = match;
    const name = namePart.trim();
    const found = progress.find((p) => p.name === name);
    if (!found) return line;

    const oldPercent = Number(oldPercentStr);
    const newPercent = found.percent;
    if (newPercent === oldPercent) return line;

    changes.push({ name, oldPercent, newPercent });
    return `- ${iconFor(newPercent)}${namePart}${newPercent}%${rest}`;
  });
  return { content: lines.join("\n"), changes };
}

function appendChangelogSection(content, changes) {
  if (changes.length === 0) return content;
  const bullets = changes
    .map((c) => `* ${c.name} : ${c.oldPercent}% → ${c.newPercent}%`)
    .join("\n");
  const section = `\n### Localisation\n${bullets}\n`;
  return content.replace(CHANGELOG_CLOSE_TAG, `${section}${CHANGELOG_CLOSE_TAG}`);
}

if (!PROJECT_ID || !TOKEN) {
  console.error("Missing CROWDIN_PROJECT_ID or CROWDIN_PERSONAL_TOKEN env vars.");
  process.exit(1);
}

const raw = readFileSync(FILE_PATH, "utf8");
const progress = await fetchProgress();
const { content: withProgress, changes } = updateLines(raw, progress);

if (changes.length === 0) {
  console.log("No translation progress changes.");
  process.exit(0);
}

const final = appendChangelogSection(withProgress, changes);
writeFileSync(FILE_PATH, final);

console.log(`Updated ${changes.length} language(s):`);
for (const c of changes) {
  console.log(`  - ${c.name}: ${c.oldPercent}% -> ${c.newPercent}%`);
}
