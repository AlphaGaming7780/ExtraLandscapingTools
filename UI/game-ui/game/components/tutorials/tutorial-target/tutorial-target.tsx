import { getModule } from "cs2/modding"
import { PropsWithChildren } from "react"

const path$ = "game-ui/game/components/tutorials/tutorial-target/tutorial-target.tsx"

export type PropsTutorialTarget = { uiTag: string, active?: boolean, disableBlinking?: boolean, editor?: boolean, children: JSX.Element }

export function TutorialTarget(propsTutorialTarget: PropsTutorialTarget): JSX.Element {
    return getModule(path$, "TutorialTarget")(propsTutorialTarget)
}
