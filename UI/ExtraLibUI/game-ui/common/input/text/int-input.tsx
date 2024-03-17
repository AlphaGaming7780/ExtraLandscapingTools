import { ExtraLibUI } from "../../../../ExtraLibUI"

const path = "game-ui/common/input/text/int-input.tsx"

export type PropsIntInput = {
	min?: number,
	max?: number
}

export function IntInput(propsIntInput: PropsIntInput) : JSX.Element
{
	return ExtraLibUI.instance.registryData.registry.get(path)?.IntInput(propsIntInput)
}