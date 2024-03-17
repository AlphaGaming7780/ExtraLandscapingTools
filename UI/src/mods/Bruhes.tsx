import { BindingListener, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { ModuleRegistryExtend } from "cs2/modding";
import { Brush, Entity, tool } from "cs2/bindings"
import { Dropdown, DropdownItem, DropdownToggle, FOCUS_AUTO, FOCUS_DISABLED } from "cs2/ui";
import { Children, ReactNode, createElement } from "react";
import { VanillaComponentResolver, PopupValueField, PropsSection } from "./test";
import { entityEquals, entityKey, shallowEqual } from "cs2/utils";



export const BrushesOptionsTool: ModuleRegistryExtend = (Component : any) => {	
	return (props) => {

		let allowBrush : boolean= useValue(tool.allowBrush$);
		let brushes : Brush[] = useValue(tool.brushes$);
		let selectedBrush : Entity = useValue(tool.selectedBrush$);
		var reactNode : JSX.Element[] = [];

		function GetSelectedBrushName() : string { // brushes : Brush[], entity: Entity
			for(let i = 0; i < brushes.length; i++) {
				if(entityEquals(brushes[i].entity, selectedBrush)) {
					return brushes[i].name
				}
			}
			return entityKey(selectedBrush)
		}

		var dropdownToggle = DropdownToggle({style: {"width": "80%"}, children: GetSelectedBrushName()})
		
		var dropDown = Dropdown({focusKey: FOCUS_DISABLED, theme: {dropdownToggle: "picker-toggle_d6k", dropdownPopup: "picker-popup_pUb", dropdownMenu: "", dropdownItem: "list-item_qRg item_H00", scrollable: "item-picker_ORP"}, content: reactNode, children: dropdownToggle})

		var propsSection : PropsSection = {
			title: "Brushes",
			children: dropDown
		}

		brushes.forEach(brush => {

			var element = createElement(
				'div',
				{ className: 'name_u39' },
				brush.name
			)

			reactNode.push(DropdownItem<Entity>({focusKey: FOCUS_AUTO, value: brush.entity, selected: entityEquals(tool.selectedBrush$.value, brush.entity), theme: {dropdownItem:"list-item_qRg item_H00"}, closeOnSelect: false, children: element, onChange: tool.selectBrush}));
		});

		// translation handling. Translates using locale keys that are defined in C# or fallback string here.
		// const { translate } = useLocalization();
		
		// This defines aspects of the components.
		const { children, ...otherProps} = props || {};

		// This gets the original component that we may alter and return.
		var result : JSX.Element = Component();

		console.log("TEST")
		console.log(allowBrush)

		if (allowBrush && selectedBrush.index != 0) {

			result.props.children?.unshift(
				VanillaComponentResolver.instance.Section(propsSection)
			);
		}
		return result;
	};
}