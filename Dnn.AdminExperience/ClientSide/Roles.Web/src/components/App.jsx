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
		logger.debug('>>> Entering constructor()');
        canEdit = util.settings.isHost || util.settings.isAdmin || util.settings.permissions.EDIT;
        this.rolesPanelref = React.createRef();
		logger.debug(`constructor(): canEdit → ${canEdit}`);
		logger.debug('<<< Exiting constructor()');
    }

    onCreate() {
		logger.debug('>>> Entering onCreate()');
        this.rolesPanelref.current.onAddRole();
		logger.debug('<<< Exiting onCreate()');
    }

    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
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
	logger.debug('>>> Entering mapStateToProps()');
	logger.debug('<<< Exiting mapStateToProps()');
    return {};
}


export default connect(mapStateToProps)(Root);