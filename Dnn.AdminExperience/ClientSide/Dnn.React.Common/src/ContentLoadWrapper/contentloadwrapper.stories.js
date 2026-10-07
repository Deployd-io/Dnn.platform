import React, { Component } from "react";
import ContentLoadWrapper from "./index";
const logger = console;
import { TableEmptyState } from "../SvgIcons";

class MyContentLoadWrapper extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = { loaded: false };    
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        setTimeout(function () {
            this.setState({ loaded: true });
        }.bind(this), 2000);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <ContentLoadWrapper
                loadComplete={this.state.loaded}
                svgSkeleton={<div><TableEmptyState /></div>}
            >
                <div>
                    <div className="auditCheckItems">
                        <ul>
                            <li>Item 1</li>
                            <li>Item 2</li>
                            <li>Item 3</li>
                        </ul>
                    </div>
                </div>
            </ContentLoadWrapper>  
        );
    }
}

export default {
    component: ContentLoadWrapper,
};

export const WithLoading = () => (
    <ContentLoadWrapper
        loadComplete={false}
        svgSkeleton={<div><TableEmptyState /></div>}
    >
        <div>
            <div className="auditcheck-topbar">Loading...</div>
            <div className="auditCheckItems">
                <ul>
                    <li>Item 1</li>
                    <li>Item 2</li>
                    <li>Item 3</li>
                </ul>
            </div>
        </div>
    </ContentLoadWrapper>
);


export const WithContent = () => (
    <ContentLoadWrapper
        loadComplete={true}
        svgSkeleton={<div><TableEmptyState /></div>}
    >
        <div>
            <div className="auditcheck-topbar">
                <h1>Content</h1>
            </div>

            <div className="auditCheckItems">
                <ul>
                    <li>Item 1</li>
                    <li>Item 2</li>
                    <li>Item 3</li>
                </ul>
            </div>
        </div>
    </ContentLoadWrapper>
);

export const LoadedAfterTwoSeconds= () => (
    <MyContentLoadWrapper />
);