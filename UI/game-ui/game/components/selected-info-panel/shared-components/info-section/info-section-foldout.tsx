import { FocusKey } from "cs2/ui"
import { getModule } from "cs2/modding"
import { ReactNode } from "react"

const path$ = "game-ui/game/components/selected-info-panel/shared-components/info-section/info-section-foldout.tsx"


export type PropsInfoSectionFoldout = {
    header: ReactNode,
    initialExpanded?: boolean,
    expandFromContent?: boolean,
    focusKey?: FocusKey,
    tooltip?: ReactNode,
    disableFocus?: boolean,
    className?: string,
    onToggleExpanded?: (value: boolean) => void,
    children: ReactNode
}

const InfoSectionFoldoutModule = getModule(path$, "InfoSectionFoldout");

export function InfoSectionFoldout(propsInfoSectionFoldout: PropsInfoSectionFoldout): JSX.Element {
    return < InfoSectionFoldoutModule {...propsInfoSectionFoldout} />
}