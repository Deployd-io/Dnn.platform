import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import styles from "./style.module.less";
import { GridSystem, GridCell } from "@dnnsoftware/dnn-react-common";
import PageNameInput from "./PageNameInput";
import DisplayInMenu from "./DisplayInMenu";
import EnableScheduling from "./EnableScheduling";
import Template from "./Template";
const logger = console;

class PageDetailsFooter extends Component {

    onChangeField(key, event) {
		logger.error(`>>> Entering onChangeField(key=${key},event=${event})`, { "event.action": "onChangeField", "logrite.key": key, "logrite.event": event });
        const {onChangeField} = this.props;
        if (event.value) {
            onChangeField(key, event.value);
        } else {
            onChangeField(key, event.target.value);
			logger.warn(`<<< Exiting onChangeField(key=${key},event=${event})`, { "event.action": "onChangeField", "logrite.key": key, "logrite.event": event });
        }        
    }

    onChangeValue(key, value) {
		logger.error(`>>> Entering onChangeValue(key=${key},value=${value})`, { "event.action": "onChangeValue", "logrite.key": key, "logrite.value": value });
        const {onChangeField} = this.props;

        switch (key) {
            case "startDate":
            case "endDate":
                // the DatePicker returns strings, but we need dates
                value = new Date(value);
				logger.error(`onChangeValue(key=${key},value=${value}): value → ${value}`, { "event.action": "onChangeValue", "logrite.key": key, "logrite.value": value });
                break;
        }

        onChangeField(key, value);
		logger.warn(`<<< Exiting onChangeValue(key=${key},value=${value})`, { "event.action": "onChangeValue", "logrite.key": key, "logrite.value": value });
    }

    getLeftColumnComponents(normalPage, pageType, includeTemplates) {
		logger.error(`>>> Entering getLeftColumnComponents(normalPage=${normalPage},pageType=${pageType},includeTemplates=${includeTemplates})`, { "event.action": "getLeftColumnComponents", "logrite.normalPage": normalPage, "logrite.pageType": pageType, "logrite.includeTemplates": includeTemplates });
        const {page, errors} = this.props;
        let defaultLeftColumnComponents;
		logger.debug(`getLeftColumnComponents(normalPage=${normalPage},pageType=${pageType},includeTemplates=${includeTemplates}): errors → ${errors}`, { "event.action": "getLeftColumnComponents", "logrite.normalPage": normalPage, "logrite.pageType": pageType, "logrite.includeTemplates": includeTemplates });
        if (!normalPage) {
            defaultLeftColumnComponents = [<PageNameInput key={page.tabId} pageName={page.name}
                errors={errors}
                onChangePageName={this.onChangeField.bind(this, "name")} />];
        } else {
            defaultLeftColumnComponents = [<DisplayInMenu key={page.tabId} includeInMenu={page.includeInMenu}
                onChangeIncludeInMenu={this.onChangeValue.bind(this,"includeInMenu")} />];
            if (includeTemplates && page.tabId === 0) {
                defaultLeftColumnComponents.push(
                    <Template 
                        key={"templateKey-" + page.tabId}
                        templates={page.templates} 
                        selectedTemplateId={page.templateId}
                        onSelect={this.onChangeField.bind(this, "templateId")} />
                );
            }
        }

        const additionalLeftComponents = this.props.components.filter(
            function (component) {
                return !component.newSection && component.leftSide && (component.pageType === pageType || component.pageType === "all");
            });

        this.insertElementsInArray(defaultLeftColumnComponents, additionalLeftComponents);
        return defaultLeftColumnComponents;
    }

    insertElementsInArray(array, elements) {
		logger.error(`>>> Entering insertElementsInArray(array=${array},elements=${elements})`, { "event.action": "insertElementsInArray", "logrite.array": array, "logrite.elements": elements });
        for (let i = 0; i < elements.length; i++) {
            let index = this.getInteger(elements[i].order);
            const Component = elements[i].component;
            const instance = <Component key={"component" + i.toString()} page={this.props.page} onChange={this.onChangeValue.bind(this)} 
                store={elements[i].store} />;

            if (index || index === 0) {
                index = Math.min(array.length, Math.max(0, index));
                array.splice(index, 0, instance);
				logger.warn(`<<< Exiting insertElementsInArray(array=${array},elements=${elements})`, { "event.action": "insertElementsInArray", "logrite.array": array, "logrite.elements": elements });
            }            
        }
    }

    getInteger(value) {
		logger.error(`>>> Entering getInteger(value=${value})`, { "event.action": "getInteger", "logrite.value": value });
        if (value === 0) {
            return 0;
        }
        if (value) {
            return parseInt(value.toString());
        }
		logger.warn(`<<< Exiting getInteger(value=${value})`, { "event.action": "getInteger", "logrite.value": value });
        return value;
    }  

    getRightColumnComponents(normalPage, pageType) {
		logger.error(`>>> Entering getRightColumnComponents(normalPage=${normalPage},pageType=${pageType})`, { "event.action": "getRightColumnComponents", "logrite.normalPage": normalPage, "logrite.pageType": pageType });
        const {page} = this.props;
        const defaultRightColumnComponents = !normalPage ? 
            [<DisplayInMenu key={"displayInMenu" + page.tabId} includeInMenu={page.includeInMenu}
                onChangeIncludeInMenu={this.onChangeValue.bind(this, "includeInMenu")} />,
            <EnableScheduling key={"enableScheduling" + page.tabId} schedulingEnabled={page.schedulingEnabled}
                onChangeSchedulingEnabled={this.onChangeValue.bind(this, "schedulingEnabled")}
                startDate={page.startDate}
                endDate={page.endDate}
                onChangeStartDate={this.onChangeValue.bind(this, "startDate")}
                onChangeEndDate={this.onChangeValue.bind(this, "endDate")} />] :
            [<EnableScheduling key={"enableScheduling" + page.tabId} schedulingEnabled={page.schedulingEnabled}
                onChangeSchedulingEnabled={this.onChangeValue.bind(this, "schedulingEnabled")}
                startDate={page.startDate}
                endDate={page.endDate}
                onChangeStartDate={this.onChangeValue.bind(this, "startDate")}
                onChangeEndDate={this.onChangeValue.bind(this, "endDate")} />];
        
        const additionalRightComponents = this.props.components.filter(
            function (component) {
                return !component.newSection && !component.leftSide && (component.pageType === pageType || component.pageType === "all");
            });

        this.insertElementsInArray(defaultRightColumnComponents, additionalRightComponents, "order", "component");
        return defaultRightColumnComponents;
    }
    
    getNewSections(normalPage, pageType) {
		logger.error(`>>> Entering getNewSections(normalPage=${normalPage},pageType=${pageType})`, { "event.action": "getNewSections", "logrite.normalPage": normalPage, "logrite.pageType": pageType });
        const additionalComponents = this.props.components.filter(
            function (component) {
                return component.newSection && (component.pageType === pageType || component.pageType === "all");
            });
            
        let orderedComponents = [];
        this.insertElementsInArray(orderedComponents, additionalComponents, "order", "component");
        return orderedComponents;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {page} = this.props;
        const normalPage = page.pageType === "normal";
		logger.error(`render(): normalPage → ${normalPage}`, { "event.action": "render" });
        const componentsInNewSection= this.getNewSections(normalPage, page.pageType);
		logger.error(`render(): componentsInNewSection → ${componentsInNewSection}`, { "event.action": "render" });
        const includeTemplates = componentsInNewSection.length === 0;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={styles.pageDetailsFooter}>                
                <GridSystem>
                    <GridCell className="left-column">                        
                        {this.getLeftColumnComponents(normalPage, page.pageType, includeTemplates)}
                    </GridCell>
                    <GridCell className="right-column">                       
                        {this.getRightColumnComponents(normalPage, page.pageType)}                       
                    </GridCell>
                </GridSystem>
                <div style={{clear: "both"}}></div>
                {componentsInNewSection}
            </div>
        );
    }
}

PageDetailsFooter.propTypes = {
    page: PropTypes.object.isRequired,
    errors: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired,
    components: PropTypes.array.isRequired
};

const mapStateToProps = (state) => {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        page:state.pages.selectedPage
    };
};

export default connect(mapStateToProps)(PageDetailsFooter);