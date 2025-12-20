import { FocusKey } from "cs2/ui"
import { getModule } from "cs2/modding"

const path$ = "game-ui/common/input/text/text-input.tsx"

export enum TextInputType {
    Text = "text",
    Password = "password",
}

export type PropsTextInput = React.TextareaHTMLAttributes<HTMLTextAreaElement> & {
    focusKey?: FocusKey;
    debugName?: string;
    type?: TextInputType | string; // optionnel, peut servir pour ton UI
    selectAllOnFocus?: boolean;
    vkTitle?: string;
    vkDescription?: string;
    multiline?: boolean; // peut remplacer "rows"
};

const TextInputModule = getModule(path$, "TextInput");

export function TextInput(propsTextInput: PropsTextInput) : JSX.Element
{
    return < TextInputModule {... propsTextInput } />
}