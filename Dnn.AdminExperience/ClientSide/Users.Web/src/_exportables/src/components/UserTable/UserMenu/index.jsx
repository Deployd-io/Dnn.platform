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
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            userDetails: props.userDetails,
            ChangePasswordVisible: false
        };
        this.showMenu = false;
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`, { "event.action": "constructor", "logrite.props": props });
        this.handleClick = this.handleClick.bind(this);
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    handleClick(event) {
		logger.error(`>>> Entering handleClick(event=${event})`, { "event.action": "handleClick", "logrite.event": event });
        if (!(this.rootElement && this.rootElement.contains(event.target)) && (typeof event.target.className !== "string" || (typeof event.target.className === "string" && event.target.className.indexOf("menu-item") === -1))) {
            this.props.onClose();
			logger.warn(`<<< Exiting handleClick(event=${event})`, { "event.action": "handleClick", "logrite.event": event });
        }
    }
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        document.addEventListener("click", this.handleClick, false);
        let {props} = this;
        this.showMenu = false;
        this.getUserDetails(props);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }
    componentDidUpdate() {
		logger.error('>>> Entering componentDidUpdate()', { "event.action": "componentDidUpdate" });
        if (this.props.userDetails === undefined && this.props.userDetails.userId !== this.props.userId) {
            this.showMenu = false;
            this.getUserDetails(this.props);
        }
        else {
            this.showMenu = true;
			logger.debug(`componentDidUpdate(): this.showMenu → ${this.showMenu}`, { "event.action": "componentDidUpdate" });
			logger.warn('<<< Exiting componentDidUpdate()', { "event.action": "componentDidUpdate" });
        }
    }
    getUserDetails(props) {
		logger.error(`>>> Entering getUserDetails(props=${props})`, { "event.action": "getUserDetails", "logrite.props": props });
        props.dispatch(CommonUsersActions.getUserDetails({ userId: props.userId }, (data) => {
            let userDetails = Object.assign({}, data);
            this.setState({
                userDetails
            },()=>{
                this.showMenu = true;
            });
			logger.warn(`<<< Exiting getUserDetails(props=${props})`, { "event.action": "getUserDetails", "logrite.props": props });
        }));
    }
    reload() {
		logger.error('>>> Entering reload()', { "event.action": "reload" });
        this.getUserDetails(this.props);
		logger.warn('<<< Exiting reload()', { "event.action": "reload" });
    }
    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        document.removeEventListener("click", this.handleClick, false);
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }
    sort(items, column, order) {
		logger.error(`>>> Entering sort(items=${items},column=${column},order=${order})`, { "event.action": "sort", "logrite.items": items, "logrite.column": column, "logrite.order": order });
        order = order === undefined ? "asc" : order;
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
		logger.error(`>>> Entering onItemClick(key=${key})`, { "event.action": "onItemClick", "logrite.key": key });
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
		logger.warn(`<<< Exiting onItemClick(key=${key})`, { "event.action": "onItemClick", "logrite.key": key });
    }
    
    onViewProfile() {
		logger.error('>>> Entering onViewProfile()', { "event.action": "onViewProfile" });
        utilities.closePersonaBar(() => {
            window.top.location = this.state.userDetails.profileUrl;
        });
		logger.warn('<<< Exiting onViewProfile()', { "event.action": "onViewProfile" });
    }
    onSendPasswordLink() {
		logger.error('>>> Entering onSendPasswordLink()', { "event.action": "onSendPasswordLink" });
        this.props.dispatch(CommonUsersActions.sendPasswordResetLink({ userId: this.props.userId }, () => {
            utilities.notify(Localization.get("PasswordSent"), 10000);
			logger.warn('<<< Exiting onSendPasswordLink()', { "event.action": "onSendPasswordLink" });
        }));
    }
    deleteUser() {
		logger.error('>>> Entering deleteUser()', { "event.action": "deleteUser" });
        utilities.confirm(Localization.get("DeleteUser.Confirm"), Localization.get("Delete"), Localization.get("Cancel"), () => {
            this.props.dispatch(CommonUsersActions.deleteUser({ userDetails: this.props.userDetails }, this.props.filter, () => {
                utilities.notify(Localization.get("UserDeleted"), 3000);
                this.reload();
				logger.warn('<<< Exiting deleteUser()', { "event.action": "deleteUser" });
            }));
        });
    }
    hardDeleteUser() {
		logger.error('>>> Entering hardDeleteUser()', { "event.action": "hardDeleteUser" });
        utilities.confirm(Localization.get("RemoveUser.Confirm"), Localization.get("Delete"), Localization.get("Cancel"), () => {
            this.props.dispatch(CommonUsersActions.eraseUser({ userId: this.props.userId }));
        });
		logger.warn('<<< Exiting hardDeleteUser()', { "event.action": "hardDeleteUser" });
    }
    restoreUser() {
		logger.error('>>> Entering restoreUser()', { "event.action": "restoreUser" });
        this.props.dispatch(CommonUsersActions.restoreUser({ userDetails: this.props.userDetails }, this.props.filter, () => {
            utilities.notify(Localization.get("UserRestored"), 3000);
            this.reload();
			logger.warn('<<< Exiting restoreUser()', { "event.action": "restoreUser" });
        }));
    }
    forcePasswordChange() {
		logger.error('>>> Entering forcePasswordChange()', { "event.action": "forcePasswordChange" });
        this.props.dispatch(CommonUsersActions.forceChangePassword({ userId: this.props.userId }, () => {
            utilities.notify(Localization.get("UserPasswordUpdateChanged"), 10000);
            this.reload();
			logger.warn('<<< Exiting forcePasswordChange()', { "event.action": "forcePasswordChange" });
        }));
    }
    updateAuthorizeStatus(authorized) {
		logger.error(`>>> Entering updateAuthorizeStatus(authorized=${authorized})`, { "event.action": "updateAuthorizeStatus", "logrite.authorized": authorized });
        this.props.dispatch(CommonUsersActions.updateAuthorizeStatus({ userDetails: this.props.userDetails }, authorized, this.props.filter, () => {
            utilities.notify(authorized ? Localization.get("UserAuthorized") :Localization.get("UserUnAuthorized"), 3000);
            this.reload();
			logger.warn(`<<< Exiting updateAuthorizeStatus(authorized=${authorized})`, { "event.action": "updateAuthorizeStatus", "logrite.authorized": authorized });
        }));
    }
    unLockUser() {
		logger.error('>>> Entering unLockUser()', { "event.action": "unLockUser" });
        this.props.dispatch(CommonUsersActions.unLockUser({ userDetails: this.props.userDetails }, () => {
            utilities.notify(Localization.get("UserUnLocked"), 3000);
            this.reload();
			logger.warn('<<< Exiting unLockUser()', { "event.action": "unLockUser" });
        }));
    }
    updateSuperUserStatus(setSuperUser) {
		logger.error(`>>> Entering updateSuperUserStatus(setSuperUser=${setSuperUser})`, { "event.action": "updateSuperUserStatus", "logrite.setSuperUser": setSuperUser });
        this.props.dispatch(CommonUsersActions.updateSuperUserStatus({ userId: this.props.userId, setSuperUser: setSuperUser }, this.props.filter, () => {
            this.reload();
			logger.warn(`<<< Exiting updateSuperUserStatus(setSuperUser=${setSuperUser})`, { "event.action": "updateSuperUserStatus", "logrite.setSuperUser": setSuperUser });
        }));
    }
    toggleChangePassword(close) {
		logger.error(`>>> Entering toggleChangePassword(close=${close})`, { "event.action": "toggleChangePassword", "logrite.close": close });
        const show = !this.state.ChangePasswordVisible;
        this.setState({ ChangePasswordVisible: show });
        if (close)
            this.props.onClose();
    }
    
    render() {
	logger.error('>>> Entering render()', { "event.action": "render" });

        let visibleMenus = [{ key:"ViewProfile", title:  Localization.get("ViewProfile"), index: 10 }];
		logger.error(`render(): visibleMenus → ${visibleMenus}`, { "event.action": "render" });
        
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
			logger.warn('<<< Exiting render()', { "event.action": "render" });
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
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        userDetails: state.users.userDetails
    };
}

export default connect(mapStateToProps)(UserMenu);