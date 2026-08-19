import React, { Component } from "react";
import ContentLoadWrapper from "./index";
const logger = console;
import { TableEmptyState } from "../SvgIcons";

class MyContentLoadWrapper extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = { loaded: false };    
		logger.debug(`constructor(): this.state → ${this.state}`);
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        setTimeout(function () {
			logger.debug('>>> Entering setTimeout()');
			logger.debug('<<< Exiting setTimeout()');
            this.setState({ loaded: true });
        }.bind(this), 2000);
		logger.debug('<<< Exiting componentDidMount()');
    }

    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
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