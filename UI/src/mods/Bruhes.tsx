import { useValue } from "cs2/api";
import { Entity, tool } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";
import { ModuleRegistryExtend } from "cs2/modding";
import { Dropdown, DropdownItem, DropdownToggle } from "cs2/ui";
import { entityEquals, entityKey } from "cs2/utils";
import { FOCUS_DISABLED$ } from "../../game-ui/common/focus/focus-key";
import { PropsSlider, Slider, SliderValueTransformer } from "../../game-ui/common/input/slider/slider";
import { PropsTextInput, TextInput, TextInputType } from "../../game-ui/common/input/text/text-input";
import { PropsSection, Section } from "../../game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options";
import { useEffect, useState } from "react";

export const BrushesOptionsTool: ModuleRegistryExtend = (Component: any) => {
    return (props) => {
        const allowBrush = useValue(tool.allowBrush$);
        const brushes = useValue(tool.brushes$);
        const selectedBrush = useValue(tool.selectedBrush$);
        const brushAngleValue = useValue(tool.brushAngle$);

        const [brushAngle, setBrushAngle] = useState(brushAngleValue)

        useEffect(() => {

            setBrushAngle(brushAngleValue);

        }, [brushAngleValue] )

        const { translate } = useLocalization();

        const selectedBrushName = brushes.find(b => entityEquals(b.entity, selectedBrush))?.name || entityKey(selectedBrush);

        // Dropdown items
        const brushItems = brushes.map((brush) => (
            <DropdownItem<Entity>
                key={entityKey(brush.entity)}
                focusKey={FOCUS_DISABLED$}
                value={brush.entity}
                selected={entityEquals(tool.selectedBrush$.value, brush.entity)}
                theme={{ dropdownItem: "list-item_qRg item_H00" }}
                closeOnSelect={false}
                onChange={tool.selectBrush}
            >
                <div className="name_u39">{brush.name}</div>
            </DropdownItem>
        ));

        const dropdown = (
            <Dropdown
                focusKey={FOCUS_DISABLED$}
                theme={{
                    dropdownToggle: "picker-toggle_d6k",
                    dropdownPopup: "picker-popup_pUb",
                    dropdownMenu: "",
                    dropdownItem: "list-item_qRg item_H00",
                    scrollable: "item-picker_ORP"
                }}
                content={brushItems }
            >
                <DropdownToggle style={{ maxWidth: "175rem" }}>{selectedBrushName}</DropdownToggle>
            </Dropdown>
        );

        const brushSectionProps: PropsSection = {
            title: translate("Toolbar.BRUSH"),
            children: dropdown
        };

        const brushSliderProps: PropsSlider = {
            focusKey: FOCUS_DISABLED$,
            value: isNaN(brushAngle) ? 0 : brushAngle,
            start: 0,
            end: 360,
            gamepadStep: 1,
            valueTransformer: SliderValueTransformer.intTransformer,
            disabled: false,
            noFill: false,
            onChange: tool.setBrushAngle
        };

        const brushAngleInputProps: PropsTextInput = {
            style: {
                height: "var(--size)"
            },
            focusKey: FOCUS_DISABLED$,
            selectAllOnFocus: true,
            type: TextInputType.Text,
            disabled: false,
            value: isNaN(brushAngle) ? "" : brushAngle.toString(),
            className: "slider-input_DXM input_Wfi",
            onChange: (e) => {
                setBrushAngle(parseInt(e.target.value, 10));
            },
            onBlur: (e) => {
                var number = parseInt(e.target.value, 10);
                if (isNaN(number)) number = 0;
                tool.setBrushAngle(number);
            }
        };

        const brushRotationSectionProps: PropsSection = {
            title: translate("Toolbar.BRUSH_ROTATION"),
            children: (
                <>
                    <div className="slider-container_Q_K" style={{ maxWidth: "110rem" }}>
                        <Slider {...brushSliderProps} />
                    </div>
                    <TextInput {...brushAngleInputProps} />
                </>
            )
        };

        const originalComponent = Component(props);
        if (allowBrush && selectedBrush.index !== 0) {
            const children = originalComponent.props.children ?? [];
            originalComponent.props.children = [<Section {...brushSectionProps} />, <Section {...brushRotationSectionProps} />, ...children];
        }

        return originalComponent;
    };
};
