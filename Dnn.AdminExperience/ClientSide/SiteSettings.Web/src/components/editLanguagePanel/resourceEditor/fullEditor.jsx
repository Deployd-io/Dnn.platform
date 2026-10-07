import React, { Component } from "react";
import PropTypes from "prop-types";
import { Button, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";
import resx from "resources";
import Html from "../../Html";
import Undo from "./icons/undo.svg";
import Redo from "./icons/redo.svg";
import Bold from "./icons/bold.svg";
import Italic from "./icons/italic.svg";
import Underline from "./icons/underline.svg";
import StrikeThrough from "./icons/strikeThrough.svg";
import JustifyLeft from "./icons/justifyLeft.svg";
import JustifyCenter from "./icons/justifyCenter.svg";
import JustifyRight from "./icons/justifyRight.svg";
import JustifyFull from "./icons/justifyFull.svg";
import InsertUnorderedList from "./icons/insertUnorderedList.svg";
import InsertOrderedList from "./icons/insertOrderedList.svg";
import "./fullEditor.less";
const logger = console;

class FullEditor extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
        };

		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
        this.editorToolbar = [
            [
                { name: "undo", icon: Undo, },
                { name: "redo", icon: Redo, }
            ],
            [
                { name: "bold", icon: Bold, },
                { name: "italic", icon: Italic, },
                { name: "underline", icon: Underline, },
                { name: "strikeThrough", icon: StrikeThrough, }
            ],
            [
                { name: "justifyLeft", icon: JustifyLeft, },
                { name: "justifyCenter", icon: JustifyCenter, },
                { name: "justifyRight", icon: JustifyRight, },
                { name: "justifyFull", icon: JustifyFull, }
            ],
            [
                { name: "insertUnorderedList", icon: InsertUnorderedList, },
                { name: "insertOrderedList", icon: InsertOrderedList, }
            ]
        ];
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.editorControl.focus();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    execCommand(button, e) {
		logger.error(`>>> Entering execCommand(button=${button},e=${e})`, { "event.action": "execCommand", "logrite.button": button, "logrite.e": e });
        this.editorControl.focus();
        document.execCommand(button, false, null);
        
        e.preventDefault();
		logger.warn(`<<< Exiting execCommand(button=${button},e=${e})`, { "event.action": "execCommand", "logrite.button": button, "logrite.e": e });
    }

    onSave() {
		logger.error('>>> Entering onSave()', { "event.action": "onSave" });
        const { props } = this;

        if (typeof props.onChange === "function") {
            let content = this.editorControl.innerHTML;
            props.onChange(content);
			logger.warn('<<< Exiting onSave()', { "event.action": "onSave" });
        }
    }

    onCancel() {
		logger.error('>>> Entering onCancel()', { "event.action": "onCancel" });
        const { props } = this;

        props.onCancel();
		logger.warn('<<< Exiting onCancel()', { "event.action": "onCancel" });
    }

     
    renderToolbar() {
		logger.error('>>> Entering renderToolbar()', { "event.action": "renderToolbar" });
		logger.warn('<<< Exiting renderToolbar()', { "event.action": "renderToolbar" });
        return (
            <div className='fulleditor-controls'>
                {
                    this.editorToolbar.map((group, i) => {
                        return <div className='btn-group' key={i}>
                            {
                                group.map((button, i) => {
                                    const ButtonIcon = button.icon;
                                    return (
                                        <a 
                                            className='btn'
                                            data-role={button.name} 
                                            href='#'
                                            onClick={this.execCommand.bind(this, button.name)}
                                            key={i}>
                                            <ButtonIcon />
                                        </a>
                                    );
                                })
                            }
                        </div>;
                    })
                }
            </div>
        );
    }

     
    renderEditor() {
		logger.error('>>> Entering renderEditor()', { "event.action": "renderEditor" });
        const { props } = this;
		logger.warn('<<< Exiting renderEditor()', { "event.action": "renderEditor" });
        return (
            <div className='fulleditor-editor' 
                contentEditable
                ref={(e) => { this.editorControl = e; }}>
                <Html html={props.value } />
            </div>
        );
    }

    renderButtons() {
		logger.error('>>> Entering renderButtons()', { "event.action": "renderButtons" });
		logger.warn('<<< Exiting renderButtons()', { "event.action": "renderButtons" });
        return [
            <Button type="neutral" onClick={this.onCancel.bind(this)} key="first">
                <TextOverflowWrapper text={resx.get("Cancel") } maxWidth={100} />
            </Button>,
            <Button type="primary" onClick={this.onSave.bind(this)} key="second">
                <TextOverflowWrapper text={resx.get("Save") } maxWidth={100} />
            </Button>
        ];
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="dnn-language-resource-full-editor">
                {this.renderToolbar()}
                {this.renderEditor()}
                {this.renderButtons()}
            </div>
        );
    }
}

FullEditor.propTypes = {
    value: PropTypes.string,
    onChange: PropTypes.func,
    onCancel: PropTypes.func,
    className: PropTypes.string
};

FullEditor.defaultProps = {
};

export default FullEditor;
