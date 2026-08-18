import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import Menu from "./menu/Menu";
import MenuItem from "./menu/MenuItem";
import Localization from "localization";
import { CommonUsersActions } from "../../../actions";
import utilities from "utils";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import ChangePassword from "../ChangePassword";
import "./style.less";
const logger = console;
import {canManagePassword, canDeleteUser, canAuthorizeUnAuthorizeUser, canPromoteDemote} from "../../permissionHelpers.js";

class UserMenu extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            userDetails: props.userDetails,
            ChangePasswordVisible: false
        };
        this.showMenu = false;
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`);
		logger.debug(`constructor(props=${props}): showMenu → ${showMenu}`);
        this.handleClick = this.handleClick.bind(this);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    handleClick(event) {
		logger.debug(`>>> Entering handleClick(event=${event})`);
        if (!(this.rootElement && this.rootElement.contains(event.target)) && (typeof event.target.className !== "string" || (typeof event.target.className === "string" && event.target.className.indexOf("menu-item") === -1))) {
            this.props.onClose();
			logger.debug(`<<< Exiting handleClick(event=${event})`);
        }
    }
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        document.addEventListener("click", this.handleClick, false);
        let {props} = this;
        this.showMenu = false;
        this.getUserDetails(props);
		logger.debug(`componentDidMount(): showMenu → ${showMenu}`);
		logger.debug('<<< Exiting componentDidMount()');
    }
    componentDidUpdate() {
		logger.debug('>>> Entering componentDidUpdate()');
        if (this.props.userDetails === undefined && this.props.userDetails.userId !== this.props.userId) {
            this.showMenu = false;
            this.getUserDetails(this.props);
        }
        else {
            this.showMenu = true;
			logger.debug('<<< Exiting componentDidUpdate()');
        }
    }
    getUserDetails(props) {
		logger.debug(`>>> Entering getUserDetails(props=${props})`);
        props.dispatch(CommonUsersActions.getUserDetails({ userId: props.userId }, (data) => {
            let userDetails = Object.assign({}, data);
            this.setState({
                userDetails
            },()=>{
                this.showMenu = true;
            });
			logger.debug(`<<< Exiting getUserDetails(props=${props})`);
        }));
    }
    reload() {
		logger.debug('>>> Entering reload()');
        this.getUserDetails(this.props);
		logger.debug('<<< Exiting reload()');
    }
    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        document.removeEventListener("click", this.handleClick, false);
		logger.debug('<<< Exiting componentWillUnmount()');
    }
    sort(items, column, order) {
        order = order === undefined ? "asc" : order;
		logger.debug(`>>> Entering sort(order=${order})`);
        items = items.sort(function (a, b) {
            if (a[column] > b[column]) //sort string descending
                return order === "asc" ? 1 : -1;
            if (a[column] < b[column])
                return order === "asc" ? -1 : 1;
            return 0;//default return value (no sorting)
        });
        return items;
    }
    onItemClick(key) {
		logger.debug(`>>> Entering onItemClick(key=${key})`);
        switch (key) {
            case "ResetPassword":
                this.onSendPasswordLink();
                this.props.onClose();
                break;
            case "ChangePassword":
                this.toggleChangePassword();
                break;
            case "ForceChangePassword":
                this.forcePasswordChange();
                this.props.onClose();
                break;
            case "DeleteUser":
                this.deleteUser();
                this.props.onClose();
                break;
            case "RemoveUser":
                this.hardDeleteUser();
                this.props.onClose();
                break;
            case "RestoreUser":
                this.restoreUser();
                this.props.onClose();
                break;
            case "cmdUnAuthorize":
                this.updateAuthorizeStatus(false);
                this.props.onClose();
                break;
            case "cmdAuthorize":
                this.updateAuthorizeStatus(true);
                this.props.onClose();
                break;
            case "cmdUnLock":
                this.unLockUser();
                this.props.onClose();
                break;
            case "PromoteToSuperUser":
                this.updateSuperUserStatus(true);
                this.props.onClose();
                break;
            case "DemoteToRegularUser":
                this.updateSuperUserStatus(false);
                this.props.onClose();
                break;
            case "ViewProfile":
                this.onViewProfile();
                this.props.onClose();
                break;
            default:
                if (typeof this.props.userMenuAction === "function")
                    this.props.userMenuAction(key, this.state.userDetails);
                this.props.onClose();
                break;
        }
		logger.debug(`<<< Exiting onItemClick(key=${key})`);
    }
    
    onViewProfile() {
		logger.debug('>>> Entering onViewProfile()');
        utilities.closePersonaBar(() => {
            window.top.location = this.state.userDetails.profileUrl;
        });
		logger.debug('<<< Exiting onViewProfile()');
    }
    onSendPasswordLink() {
		logger.debug('>>> Entering onSendPasswordLink()');
        this.props.dispatch(CommonUsersActions.sendPasswordResetLink({ userId: this.props.userId }, () => {
            utilities.notify(Localization.get("PasswordSent"), 10000);
			logger.debug('<<< Exiting onSendPasswordLink()');
        }));
    }
    deleteUser() {
		logger.debug('>>> Entering deleteUser()');
        utilities.confirm(Localization.get("DeleteUser.Confirm"), Localization.get("Delete"), Localization.get("Cancel"), () => {
            this.props.dispatch(CommonUsersActions.deleteUser({ userDetails: this.props.userDetails }, this.props.filter, () => {
                utilities.notify(Localization.get("UserDeleted"), 3000);
                this.reload();
				logger.debug('<<< Exiting deleteUser()');
            }));
        });
    }
    hardDeleteUser() {
		logger.debug('>>> Entering hardDeleteUser()');
        utilities.confirm(Localization.get("RemoveUser.Confirm"), Localization.get("Delete"), Localization.get("Cancel"), () => {
            this.props.dispatch(CommonUsersActions.eraseUser({ userId: this.props.userId }));
        });
		logger.debug('<<< Exiting hardDeleteUser()');
    }
    restoreUser() {
		logger.debug('>>> Entering restoreUser()');
        this.props.dispatch(CommonUsersActions.restoreUser({ userDetails: this.props.userDetails }, this.props.filter, () => {
            utilities.notify(Localization.get("UserRestored"), 3000);
            this.reload();
			logger.debug('<<< Exiting restoreUser()');
        }));
    }
    forcePasswordChange() {
		logger.debug('>>> Entering forcePasswordChange()');
        this.props.dispatch(CommonUsersActions.forceChangePassword({ userId: this.props.userId }, () => {
            utilities.notify(Localization.get("UserPasswordUpdateChanged"), 10000);
            this.reload();
			logger.debug('<<< Exiting forcePasswordChange()');
        }));
    }
    updateAuthorizeStatus(authorized) {
		logger.debug(`>>> Entering updateAuthorizeStatus(authorized=${authorized})`);
        this.props.dispatch(CommonUsersActions.updateAuthorizeStatus({ userDetails: this.props.userDetails }, authorized, this.props.filter, () => {
            utilities.notify(authorized ? Localization.get("UserAuthorized") :Localization.get("UserUnAuthorized"), 3000);
            this.reload();
			logger.debug(`<<< Exiting updateAuthorizeStatus(authorized=${authorized})`);
        }));
    }
    unLockUser() {
		logger.debug('>>> Entering unLockUser()');
        this.props.dispatch(CommonUsersActions.unLockUser({ userDetails: this.props.userDetails }, () => {
            utilities.notify(Localization.get("UserUnLocked"), 3000);
            this.reload();
			logger.debug('<<< Exiting unLockUser()');
        }));
    }
    updateSuperUserStatus(setSuperUser) {
		logger.debug(`>>> Entering updateSuperUserStatus(setSuperUser=${setSuperUser})`);
        this.props.dispatch(CommonUsersActions.updateSuperUserStatus({ userId: this.props.userId, setSuperUser: setSuperUser }, this.props.filter, () => {
            this.reload();
			logger.debug(`<<< Exiting updateSuperUserStatus(setSuperUser=${setSuperUser})`);
        }));
    }
    toggleChangePassword(close) {
		logger.debug(`>>> Entering toggleChangePassword(close=${close})`);
        const show = !this.state.ChangePasswordVisible;
        this.setState({ ChangePasswordVisible: show });
        if (close)
			logger.debug(`<<< Exiting toggleChangePassword(close=${close})`);
            this.props.onClose();
    }
    
    render() {
	logger.debug('>>> Entering render()');

        let visibleMenus = [{ key:"ViewProfile", title:  Localization.get("ViewProfile"), index: 10 }];
		logger.debug(`render(): visibleMenus → ${visibleMenus}`);
        
        if (canPromoteDemote(this.props.appSettings.applicationSettings.settings, this.state.userDetails.userId))
        {
            if (!this.state.userDetails.isSuperUser) {
                visibleMenus = [{ key:"PromoteToSuperUser", title:  Localization.get("PromoteToSuperUser"), index: 80 }].concat(visibleMenus);
            }
            else if (this.state.userDetails.isSuperUser)
            {
                visibleMenus = [{ key:"DemoteToRegularUser", title:  Localization.get("DemoteToRegularUser"), index: 80 }].concat(visibleMenus);
            }
        }
        if (canManagePassword(this.props.appSettings.applicationSettings.settings, this.state.userDetails.userId))
        {
            visibleMenus = [{ key:"ResetPassword", title: Localization.get("ResetPassword"), index: 40 }].concat(visibleMenus);
            visibleMenus = [{ key:"ChangePassword", title: Localization.get("ChangePassword"), index: 30 }].concat(visibleMenus);
            if (!this.state.userDetails.needUpdatePassword) {
                visibleMenus = [{ key:"ForceChangePassword", title:  Localization.get("ForceChangePassword"), index: 40 }].concat(visibleMenus);
            }
        }
        if (canDeleteUser(this.props.appSettings.applicationSettings.settings, this.state.userDetails.userId))
        {
            if (this.state.userDetails.isDeleted) {
                visibleMenus = [{ key:"RestoreUser", title:  Localization.get("RestoreUser"), index: 70 }].concat(visibleMenus);
                visibleMenus = [{ key:"RemoveUser", title:  Localization.get("RemoveUser"), index: 60 }].concat(visibleMenus);
            } else
            {
                visibleMenus = [{ key:"DeleteUser", title:  Localization.get("DeleteUser"), index: 60 }].concat(visibleMenus);
            }
        }
        if (canAuthorizeUnAuthorizeUser(this.props.appSettings.applicationSettings.settings, this.state.userDetails.userId))
        {
            if (this.state.userDetails.authorized) {
                visibleMenus = [{ key:"cmdUnAuthorize", title:  Localization.get("cmdUnAuthorize"), index: 50 }].concat(visibleMenus);
            } 
            else
            {
                visibleMenus = [{ key:"cmdAuthorize", title:  Localization.get("cmdAuthorize"), index: 50 }].concat(visibleMenus);
            }
            if (this.state.userDetails.isLocked) {
                visibleMenus = [{ key:"cmdUnLock", title:  Localization.get("cmUnlockUser"), index: 100 }].concat(visibleMenus);
            }
        }
       
        visibleMenus = visibleMenus.concat((this.props.getUserMenu && this.props.getUserMenu(this.state.userDetails)) || []);

        visibleMenus = this.sort(visibleMenus, "index");
        let showMenu = this.showMenu;
        
        if (showMenu)
        {
            return (
                <div ref={node => this.rootElement = node}>
                    <GridCell className="dnn-user-menu menu-popup">
                        {!this.state.ChangePasswordVisible &&
                        <Menu>
                            {
                                visibleMenus.map((menu, index) => {
                                    return <MenuItem key={`menu_item_${index}`} onMenuAction={this.onItemClick.bind(this, menu.key) }>{menu.title}</MenuItem>;
                                })
                            }
                        </Menu>
                        }
                        {this.state.ChangePasswordVisible &&
                        <ChangePassword onCancel={this.toggleChangePassword.bind(this, true) } userId={this.props.userId} />
                        }
                    </GridCell>
                </div>
            );
        }
        else
        {
			logger.debug('<<< Exiting render()');
            return <div/>;
        }
    }
}

UserMenu.propTypes = {
    dispatch: PropTypes.func.isRequired,
    userId: PropTypes.number.isRequired,
    onClose: PropTypes.func.isRequired,
    userDetails: PropTypes.object,
    getUserMenu: PropTypes.func,
    userMenuAction: PropTypes.func,
    appSettings: PropTypes.object,
    filter: PropTypes.number
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        userDetails: state.users.userDetails
    };
}

export default connect(mapStateToProps)(UserMenu);