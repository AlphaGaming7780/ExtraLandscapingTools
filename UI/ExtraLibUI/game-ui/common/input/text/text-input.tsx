import { FocusKey } from "cs2/ui"
import { ExtraLibUI } from "../../../../ExtraLibUI"
import { EventInfo } from "cs2/bindings"
import { FormEvent } from "react"

const path = "game-ui/common/input/text/text-input.tsx"

export enum TextInputType {
    Text = "text",
    Password = "password",
}

export type PropsTextInput = {
	focusKey?: FocusKey,
	debugName?: string,
    type?: TextInputType | string,
    value?: string,
    selectAllOnFocus?: boolean,
    placeholder?: string,
    vkTitle?: string,
    vkDescription?: string,
    disabled: boolean,
    className?: string,
    multiline: number,
    onFocus?: (value: Event) => void,
    onBlur?: (value: Event) => void,
    onKeyDown?: (value: Event) => void,
    onChange?: (value: Event) => void,
    onMouseUp?: (value: Event) => void,
}

export function TextInput(propsTextInput: PropsTextInput) : JSX.Element
{
	return ExtraLibUI.instance.registryData.registry.get(path)?.TextInput.render(propsTextInput)
}