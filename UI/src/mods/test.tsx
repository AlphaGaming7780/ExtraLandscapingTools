import { Theme, UniqueFocusKey } from "cs2/bindings";
import { ModuleRegistry } from "cs2/modding";
import { HTMLAttributes, ReactNode } from "react";

// These are specific to the types of components that this mod uses.
// In the UI developer tools at http://localhost:9444/ go to Sources -> Index.js. Pretty print if it is formatted in a single line.
// Search for the tsx or scss files. Look at the function referenced and then find the properies for the component you're interested in.
// As far as I know the types of properties are just guessed.
type PropsToolButton = {
	focusKey?: UniqueFocusKey | null
	src: string
	selected : boolean
	multiSelect : boolean
	disabled?: boolean
	tooltip?: string | null
	selectSound?: any
	uiTag?: string
	className?: string
	children?: string | JSX.Element | JSX.Element[]
	onSelect?: (x: any) => any,
} & HTMLAttributes<any>

export type PropsSection = {
	title?: string | null
	uiTag?: string
	children: string | JSX.Element | JSX.Element[]
}

export type PopupValueField = {label : string, value: string, expanded: boolean, disabled: boolean, children: string, onExpandedChange: Function }

// This is an array of the different components and sass themes that are appropriate for your UI. You need to figure out which ones you need from the registry.
const registryIndex = {
	Section: ["game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", "Section"],
	ToolButton: ["game-ui/game/components/tool-options/tool-button/tool-button.tsx", "ToolButton"],
	toolButtonTheme: ["game-ui/game/components/tool-options/tool-button/tool-button.module.scss", "classes"],
	
	PopupValueField: ["game-ui/editor/widgets/fields/popup-value-field.tsx", "PopupValueField"],
}

export class VanillaComponentResolver {
	// As far as I know you should not need to edit this portion here. 
	// This was written by Klyte for his mod's UI but I didn't have to make any edits to it at all. 
	public static get instance(): VanillaComponentResolver { return this._instance!! }
	private static _instance?: VanillaComponentResolver

	public static setRegistry(in_registry: ModuleRegistry) { this._instance = new VanillaComponentResolver(in_registry); }
	private registryData: ModuleRegistry;

	constructor(in_registry: ModuleRegistry) {
		this.registryData = in_registry;
	}

	private cachedData: Partial<Record<keyof typeof registryIndex, any>> = {}
	private updateCache(entry: keyof typeof registryIndex) {
		const entryData = registryIndex[entry];
		return this.cachedData[entry] = this.registryData.registry.get(entryData[0])!![entryData[1]]
	}

	// This section defines your components and themes in a way that you can access via the singleton in your components.
	// Replace the names, props, and strings as needed for your mod.
	public get Section(): (props: PropsSection) => JSX.Element { return this.cachedData["Section"] ?? this.updateCache("Section") }
	public get ToolButton(): (props: PropsToolButton) => JSX.Element { return this.cachedData["ToolButton"] ?? this.updateCache("ToolButton") }
	// public get PopupValueField(): (props: PopupValueField) => JSX.Element { return this.cachedData["PopupValueField"] ?? this.updateCache("PopupValueField") }

	public PopupValueField(PopupValueField : PopupValueField) : ReactNode {
		var test : (Record<string, any> | undefined) = this.registryData.registry.get("game-ui/editor/widgets/fields/popup-value-field.tsx")

		console.log(test);
	
		if(test !== undefined) {
	
			return test.PopupValueField(PopupValueField);
			// test.BoundPopupValueField();
		}
	}

	public get toolButtonTheme(): Theme | any { return this.cachedData["toolButtonTheme"] ?? this.updateCache("toolButtonTheme") }

} 