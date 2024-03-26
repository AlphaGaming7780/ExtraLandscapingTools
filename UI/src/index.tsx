import { ModRegistrar } from "cs2/modding";
import { BrushesOptionsTool } from "mods/Bruhes";
import { HelloWorldComponent } from "mods/hello-world";
//import { ExtraLibUI } from "../ExtraLibUI/ExtraLibUI";

const register: ModRegistrar = (moduleRegistry) => {
    // While launching game in UI development mode (include --uiDeveloperMode in the launch options)
    // - Access the dev tools by opening localhost:9444 in chrome browser.
    // - You should see a hello world output to the console.
    // - use the useModding() hook to access exposed UI, api and native coherent engine interfaces. 
    // Good luck and have fun!

    //ExtraLibUI.setRegistry(moduleRegistry)

    moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', BrushesOptionsTool);

    moduleRegistry.append('Menu', HelloWorldComponent);
}

export default register;