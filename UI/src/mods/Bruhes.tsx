import { useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { ModuleRegistryExtend } from "cs2/modding";
import { Brush, Entity, tool } from "cs2/bindings"
import { Dropdown, DropdownItem, DropdownToggle, FOCUS_AUTO, FOCUS_DISABLED } from "cs2/ui";
import { createElement } from "react";
import { entityEquals, entityKey } from "cs2/utils";
import { PropsSlider, SliderValueTransformer, Slider } from "../../ExtraLibUI/game-ui/common/input/slider/slider";
import { PropsSection, Section } from "../../ExtraLibUI/game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options";
import { PropsTextInput, TextInput, TextInputType } from "../../ExtraLibUI/game-ui/common/input/text/text-input";


export const BrushesOptionsTool: ModuleRegistryExtend = (Component : any) => {	
	return (props) => {

		let allowBrush : boolean= useValue(tool.allowBrush$);
		let brushes : Brush[] = useValue(tool.brushes$);
		let selectedBrush : Entity = useValue(tool.selectedBrush$);
		let brushAngle : number = useValue(tool.brushAngle$);
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
			title: "Brush",
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

		var propsSlider : PropsSlider = {
			focusKey: FOCUS_DISABLED,
			value: brushAngle,
			start: 0,
			end: 360,
			gamepadStep: 1,
			valueTransformer: SliderValueTransformer.intTransformer,
			disabled: false,
			noFill: false,
			onChange: function(number) {tool.setBrushAngle(number)},
			// onDragStart: function() {console.log("onDragStart")},
			// onDragEnd: function() {console.log("onDragEnd")},
			// onMouseOver: function() {console.log("onMouseOver")},
			// onMouseLeave: function() {console.log("onMouseLeave")}
		}

		let propsTextInput : PropsTextInput = {
			focusKey: FOCUS_DISABLED,
			type: TextInputType.Text,
			disabled: false,
			multiline: 1,
			value: brushAngle.toString(),
			className: "slider-input_DXM input_Wfi",
			onChange(value) {
				if(value?.target instanceof HTMLTextAreaElement) {
					let number : number = parseInt(value.target.value, 10)
					tool.setBrushAngle(number)
					// console.log(number)
				}
			},
		}

		var sliderPropsSection : PropsSection = {
			title: "Brush Rotation",
			children: [
				<div className="slider-container_Q_K" style={{width:"27.5%"}}>{Slider(propsSlider)}</div>,
				TextInput(propsTextInput)
			]
		}

		if (allowBrush && selectedBrush.index != 0) {

			result.props.children?.unshift(
				Section(propsSection),
				Section(sliderPropsSection)
				
			);
		}
		return result;
	};
}