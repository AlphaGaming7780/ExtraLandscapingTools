import { ExtraLibUI } from "../../../../../ExtraLibUI"

const path = "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx"

export type PropsSection = {
	title?: string | null
	uiTag?: string
	children: string | JSX.Element | JSX.Element[]
}

export function Section(propsSection: PropsSection) : JSX.Element
{
    return ExtraLibUI.instance.registryData.registry.get(path)?.Section(propsSection)
}

export function MouseToolOptions() : JSX.Element
{
    return ExtraLibUI.instance.registryData.registry.get(path)?.MouseToolOptions()
}
