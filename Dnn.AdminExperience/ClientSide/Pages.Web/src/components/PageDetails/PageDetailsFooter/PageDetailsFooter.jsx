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
        const {onChangeField} = this.props;
        if (event.value) {
            onChangeField(key, event.value);
        } else {
            onChangeField(key, event.target.value);
        }        
		logger.debug(`<<< Exiting onChangeField(key=${key},event=${event})`);
    }

    onChangeValue(key, value) {
        const {onChangeField} = this.props;

        switch (key) {
            case "startDate":
            case "endDate":
                // the DatePicker returns strings, but we need dates
                value = new Date(value);
                break;
        }

        onChangeField(key, value);
		logger.debug(`<<< Exiting onChangeValue(key=${key},value=${value})`);
    }

    getLeftColumnComponents(normalPage, pageType, includeTemplates) {
        const {page, errors} = this.props;
        let defaultLeftColumnComponents;
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
				logger.debug(`>>> Entering function(component=${component})`);
				logger.debug(`<<< Exiting function(component=${component})`);
                return !component.newSection && component.leftSide && (component.pageType === pageType || component.pageType === "all");
            });

        this.insertElementsInArray(defaultLeftColumnComponents, additionalLeftComponents);
        return defaultLeftColumnComponents;
    }

    insertElementsInArray(array, elements) {
		logger.debug(`>>> Entering insertElementsInArray(array=${array},elements=${elements})`);
        for (let i = 0; i < elements.length; i++) {
			logger.debug(`<<< Exiting insertElementsInArray(array=${array},elements=${elements})`);
            let index = this.getInteger(elements[i].order);
            const Component = elements[i].component;
            const instance = <Component key={"component" + i.toString()} page={this.props.page} onChange={this.onChangeValue.bind(this)} 
                store={elements[i].store} />;

            if (index || index === 0) {
                index = Math.min(array.length, Math.max(0, index));
                array.splice(index, 0, instance);
            }            
        }
    }

    getInteger(value) {
		logger.debug(`>>> Entering getInteger(value=${value})`);
        if (value === 0) {
            return 0;
        }
        if (value) {
		logger.debug(`<<< Exiting getInteger(value=${value})`);
            return parseInt(value.toString());
        }
        return value;
    }  

    getRightColumnComponents(normalPage, pageType) {
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
        const additionalComponents = this.props.components.filter(
            function (component) {
                return component.newSection && (component.pageType === pageType || component.pageType === "all");
            });
            
        let orderedComponents = [];
        this.insertElementsInArray(orderedComponents, additionalComponents, "order", "component");
        return orderedComponents;
    }

    render() {
		logger.debug('>>> Entering render()');
        const {page} = this.props;
        const normalPage = page.pageType === "normal";
		logger.debug(`render(): normalPage → ${normalPage}`);
        const componentsInNewSection= this.getNewSections(normalPage, page.pageType);
		logger.debug(`render(): componentsInNewSection → ${componentsInNewSection}`);
        const includeTemplates = componentsInNewSection.length === 0;
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        page:state.pages.selectedPage
    };
};

export default connect(mapStateToProps)(PageDetailsFooter);