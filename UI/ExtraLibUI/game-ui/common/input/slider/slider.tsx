import { FocusKey } from "cs2/ui"
import { ReactNode } from "react"
import { ExtraLibUI } from "../../../../ExtraLibUI"

export type PropsSlider = {
	focusKey?: FocusKey, 
	debugName?: string, 
	value: number, 
	start: number, 
	end: number, 
	gamepadStep: number, 
	disabled: boolean, 
	vertical?: boolean, 
	sounds?: SliderSounds, 
	thumb?: ReactNode, 
	theme?: any, 
	className?: string, 
	style?: string, 
	children?: string, 
	noFill: boolean, 
	valueTransformer?: SliderValueTransformer, 
	onChange?: (value: number) => void, 
	onDragStart?: () => void, 
	onDragEnd?: () => void, 
	onMouseOver?: () => void, 
	onMouseLeave?: () => void
}
export type SliderSounds = {dragStart: string, drag: string, scaleDragVolume: boolean}
export enum SliderValueTransformer {floatTransformer, intTransformer, useStepTransformer}

export function Slider(propsSlider: PropsSlider) : JSX.Element
{
	if(propsSlider.valueTransformer == SliderValueTransformer.floatTransformer) propsSlider.valueTransformer = undefined
	else if(propsSlider.valueTransformer == SliderValueTransformer.intTransformer) propsSlider.valueTransformer = ExtraLibUI.instance.registryData.registry.get("game-ui/common/input/slider/slider.tsx")?.intTransformer
	else if(propsSlider.valueTransformer == SliderValueTransformer.useStepTransformer) propsSlider.valueTransformer = ExtraLibUI.instance.registryData.registry.get("game-ui/common/input/slider/slider.tsx")?.useStepTransformer

	return ExtraLibUI.instance.registryData.registry.get("game-ui/common/input/slider/slider.tsx")?.Slider.render(propsSlider)

}



