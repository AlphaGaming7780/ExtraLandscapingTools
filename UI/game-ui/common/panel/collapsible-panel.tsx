import { getModule } from "cs2/modding"

const path$ = "game-ui/common/panel/collapsible-panel.tsx"

export type PropsCollapsiblePanel = {
    theme?: any,
    onClose?: () => void,
    expanded?: boolean,
    headerText: string | null,
    onToggleExpanded?: () => void,
    className?: string,
    children?: any,
    isFocusRoot?: any,
    headerIcon?: any,
    togglable?: boolean,
    [key: string]: any;
}

const CollapsiblePanelModule = getModule(path$, "CollapsiblePanel");

export function CollapsiblePanel(propsCollapsiblePanel: PropsCollapsiblePanel): JSX.Element {
    return <CollapsiblePanelModule {...propsCollapsiblePanel} />
}