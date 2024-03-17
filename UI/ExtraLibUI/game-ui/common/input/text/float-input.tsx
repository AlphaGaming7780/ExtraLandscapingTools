import { ExtraLibUI } from "../../../../ExtraLibUI"

const path = "game-ui/common/input/text/float-input.tsx"

export type PropsFloatInput = {
    min?: number,
    max?: number,
    fractionDigits?: number
}

export function IntInput(propsFloatInput: PropsFloatInput) : JSX.Element
{
    return ExtraLibUI.instance.registryData.registry.get(path)?.FloatInput(propsFloatInput)
}
