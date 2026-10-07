import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Button, PersonaBarPageHeader, PersonaBarPage}  from "@dnnsoftware/dnn-react-common";
import RolesPanel from "./roles";
import resx from "../resources";
import util from "utils";
const logger = console;
let canEdit = false;
class Root extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        canEdit = util.settings.isHost || util.settings.isAdmin || util.settings.permissions.EDIT;
        this.rolesPanelref = React.createRef();
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    onCreate() {
		logger.error('>>> Entering onCreate()', { "event.action": "onCreate" });
        this.rolesPanelref.current.onAddRole();
		logger.warn('<<< Exiting onCreate()', { "event.action": "onCreate" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="roles-app">
                <PersonaBarPage isOpen={true}>
                    <PersonaBarPageHeader title={resx.get("Roles") }>
                        {canEdit &&
                            <Button type="primary" size="large" onClick={this.onCreate.bind(this) }>{resx.get("Create") }</Button>
                        }
                    </PersonaBarPageHeader>
                    <RolesPanel ref={this.rolesPanelref} />
                </PersonaBarPage>
            </div>
        );
    }
}

Root.propTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number
};


function mapStateToProps() {
	logger.error('>>> Entering mapStateToProps()', { "event.action": "mapStateToProps" });
	logger.warn('<<< Exiting mapStateToProps()', { "event.action": "mapStateToProps" });
    return {};
}


export default connect(mapStateToProps)(Root);