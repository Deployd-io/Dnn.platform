import PropTypes from "prop-types";
import React, { Component } from "react";
import styles from "./style.module.less";
import {formatDate, sort} from "../../../helpers";
import UserMenu from "../UserMenu";
import Localization from "localization";
import ColumnSizes from "../columnSizes";
const logger = console;
import {canManageRoles, canManageProfile, canViewSettings} from "../../permissionHelpers.js";
import { SvgIcons, GridCell, Collapsible, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";

class DetailsRow extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.rootElement = React.createRef();
        this.state = {
            opened: false,
            showMenu: false
        };
    }
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this._isMounted = true;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        this._isMounted = false;
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    handleClick(event) {
		logger.error(`>>> Entering handleClick(event=${event})`, { "event.action": "handleClick", "logrite.event": event });
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }
        if ((typeof event.target.className === "string" && event.target.className.indexOf("do-not-close") === -1)
            && !(event.target.id === "confirmbtn" || event.target.id === "cancelbtn") && this.props.openId !== "add") {
            if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
                this.props.Collapse();
				logger.warn(`<<< Exiting handleClick(event=${event})`, { "event.action": "handleClick", "logrite.event": event });
            }
        }
    }
    toggle(index) {
		logger.error(`>>> Entering toggle(index=${index})`, { "event.action": "toggle", "logrite.index": index });
        if ((this.props.openId !== "" && this.props.id === this.props.openId) && this.props.currentIndex === index) {
            this.props.Collapse();
        } else {
            this.props.OpenCollapse(this.props.id, index);
			logger.warn(`<<< Exiting toggle(index=${index})`, { "event.action": "toggle", "logrite.index": index });
        }
    }
    toggleUserMenu() {
		logger.error('>>> Entering toggleUserMenu()', { "event.action": "toggleUserMenu" });
        const show = !this.state.showMenu;
        this.setState({ showMenu: show });
		logger.warn('<<< Exiting toggleUserMenu()', { "event.action": "toggleUserMenu" });
    }
    
    /* eslint-disable react/no-danger */
    getUserActions(user, opened) {
		logger.error(`>>> Entering getUserActions(user=${user},opened=${opened})`, { "event.action": "getUserActions", "user.name": String(user), "logrite.opened": opened });
        let actionIcons = [];
        actionIcons = actionIcons.concat((this.props.getUserTabsIcons && this.props.getUserTabsIcons(user)) || []);
		logger.error(`getUserActions(user=${user},opened=${opened}): actionIcons → ${actionIcons}`, { "event.action": "getUserActions", "user.name": String(user), "logrite.opened": opened });

        if (canManageProfile(this.props.appSettings.applicationSettings.settings))
        {
            actionIcons = actionIcons.concat([{
                index: 15,
                icon: SvgIcons.UserIcon,
                title: Localization.get("ManageProfile.title")
            }]);
        }
        if (canViewSettings(this.props.appSettings.applicationSettings.settings)) {
            actionIcons = actionIcons.concat([{
                index: 10,
                icon: SvgIcons.SettingsIcon,
                title: Localization.get("ManageSettings.title")
            }]);
        }

        if (canManageRoles(this.props.appSettings.applicationSettings.settings, user))
        {
            actionIcons = actionIcons.concat([{
                index: 5,
                icon: SvgIcons.ShieldIcon,
                title: Localization.get("ManageRoles.title")
            }]);
        }

        let i = 0;
        let userActions = sort(actionIcons, "index", "desc").map((actionIcon) => {
            let Icon = actionIcon.icon;
            let element = <div key={`user_action_${i}`} title={actionIcon.title} className={ "extension-action " + !(opened && this.props.currentIndex === i) } onClick={ this.toggle.bind(this, i) } ><Icon /></div>;
			logger.error(`getUserActions(user=${user},opened=${opened}): element → ${element}`, { "event.action": "getUserActions", "user.name": String(user), "logrite.opened": opened });
            i++;
		logger.warn(`<<< Exiting getUserActions(user=${user},opened=${opened})`, { "event.action": "getUserActions", "user.name": String(user), "logrite.opened": opened });
            return element;
        });
        return ([<div key={`user_action_wrapper_${user.userId}`} style={{ position: "relative" }}>
            <div className={"extension-action " + !this.state.showMenu}
                onClick={this.toggleUserMenu.bind(this) }>
                    <SvgIcons.MoreMenuIcon />
            </div>
            { this.state.showMenu && <UserMenu filter={this.props.filter} appSettings={this.props.appSettings} getUserMenu={this.props.getUserMenu && this.props.getUserMenu.bind(this)} userMenuAction={this.props.userMenuAction && this.props.userMenuAction.bind(this)} onClose={this.toggleUserMenu.bind(this) } 
                userId={user.userId}/> }
        </div>]).concat(userActions);
    }
    getUserColumns(user, id, opened) {
		logger.error(`>>> Entering getUserColumns(user=${user},id=${id},opened=${opened})`, { "event.action": "getUserColumns", "user.name": String(user), "logrite.id": id, "logrite.opened": opened });
        let userActions = this.getUserActions(user, opened);
        let extraColumns = this.props.getUserColumns && this.props.getUserColumns(user);
		logger.error(`getUserColumns(user=${user},id=${id},opened=${opened}): extraColumns → ${extraColumns}`, { "event.action": "getUserColumns", "user.name": String(user), "logrite.id": id, "logrite.opened": opened });
        let columnSizes =this.props.columnSizes!==undefined? this.props.columnSizes: ColumnSizes;
        let userColumns = [];
        if (this.props.appSettings.applicationSettings.settings.dataConsentActive) {
            let statusClass = "black";
			logger.error(`getUserColumns(user=${user},id=${id},opened=${opened}): statusClass → ${statusClass}`, { "event.action": "getUserColumns", "user.name": String(user), "logrite.id": id, "logrite.opened": opened });
            let StatusIcon = SvgIcons.Signature;
            let hoverText = Localization.get("HasAgreedToTerms.title");
			logger.error(`getUserColumns(user=${user},id=${id},opened=${opened}): hoverText → ${hoverText}`, { "event.action": "getUserColumns", "user.name": String(user), "logrite.id": id, "logrite.opened": opened });
            if (user.requestsRemoval) {
                statusClass = "red";
                StatusIcon = SvgIcons.UserSlash;
                hoverText = Localization.get("RequestsRemoval.title");
            } else if (user.isDeleted) {
                statusClass = "grey";
                StatusIcon = SvgIcons.UserSlash;
                hoverText = Localization.get("Deleted");
            } else if (!user.authorized) {
                statusClass = "grey";
                StatusIcon = SvgIcons.CrossOutIcon;
                hoverText = Localization.get("UnAuthorized");
            } else if (!user.hasAgreedToTerms) {
                statusClass = "grey";
                hoverText = Localization.get("HasNotAgreedToTerms.title");
            } 
            userColumns = [
                {
                    index: 3,
                    content: <GridCell key={`gc-userstatus-${user.userId}`} columnSize={columnSizes.find(x=>x.index===3).size}>
                        <span className={"user-status " + statusClass} title={hoverText}><StatusIcon /></span>
                    </GridCell>
                }];
        }
        userColumns = userColumns.concat([
            {
                index: 5,
                content:
                <GridCell key={`gc-username-${user.userId}`} columnSize={columnSizes.find(x=>x.index===5).size}  className={"user-names" + (user.isDeleted ? " deleted" : "") }>
                    <div className="user-name">
                        {user.avatar !== "-" &&
                            <img src={user.avatar} alt={user.userName} />
                        }
                        <div>
                            <h6>
                                <TextOverflowWrapper className="email-link" text={user.displayName} maxWidth={125}/>
                            </h6>
                            {user.displayName !== "-" && <p title={`${Localization.get("UserId")} ${user.userId}`}>{user.userName}</p> }
                        </div>
                    </div>
                </GridCell>
            },
            {
                index: 10,
                content: <GridCell key={`gc-email-link-${user.userId}`} columnSize={columnSizes.find(x=>x.index===10).size}  className={"user-emails" + (user.isDeleted ? " deleted" : "") } >
                    <TextOverflowWrapper className="email-link" isAnchor={true} href={"mailto:" + user.email} text={user.email} maxWidth={125}/>
                </GridCell>
            },
            {
                index: 15,
                content: <GridCell key={`gc-createdon-${user.userId}`} columnSize={columnSizes.find(x=>x.index===15).size}  className={"user-joined" + (user.isDeleted ? " deleted" : "")}>
                    {user.createdOnDate !== "-" && <p>{formatDate(user.createdOnDate) }</p>}
                    {user.createdOnDate === "-" && user.createdOnDate}
                </GridCell>
            },
            {
                index: 25,
                content: id !== "add" && <GridCell key={`gc-actions-${user.userId}`} columnSize={columnSizes.find(x=>x.index===25).size} style={{float:"right", textAlign:"right", paddingRight: 2}}>{userActions}</GridCell>
            }
        ]).concat((extraColumns) || []);

		logger.warn(`<<< Exiting getUserColumns(user=${user},id=${id},opened=${opened})`, { "event.action": "getUserColumns", "user.name": String(user), "logrite.id": id, "logrite.opened": opened });
        return sort(userColumns, "index").map((column) => {
            return column.content;
        });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
        let {user} = this.props;
		logger.debug(`render(): user → ${user}`, { "event.action": "render" });
        let opened = (props.openId !== "" && props.id === props.openId);
		logger.error(`render(): opened → ${opened}`, { "event.action": "render" });
        let uniqueId = "userRow-" + Math.random() + Date.now();
		logger.error(`render(): uniqueId → ${uniqueId}`, { "event.action": "render" });
        if (user === undefined) {
            user = {
                avatar: "-",
                displayName: "-",
                userName: "-",
                email: "-",
                createdOnDate: "-",
                authorized: "-"
            };
        }
        let userColumns = this.getUserColumns(user, props.id, opened);
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            /* eslint-disable react/no-danger */
            <GridCell className={"collapsible-component-users"} id={uniqueId} ref={(node) => this.rootElement = node}>
                <GridCell className={"collapsible-header-users " + !opened}>
                    <GridCell className={styles.extensionDetailRow + " " + props.addIsOpened} columnSize={100}>
                        {(!props.addIsOpened || props.addIsOpened === "add-opened") &&
                        <GridCell>
                            {userColumns}
                        </GridCell>}
                        <Collapsible accordion={true} isOpened={opened} className="user-detail-row">
                            {opened && props.children }
                        </Collapsible>
                    </GridCell>
                </GridCell>
            </GridCell>
        );
    }
}

DetailsRow.propTypes = {
    user: PropTypes.object,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    currentIndex: PropTypes.number,
    getUserTabsIcons: PropTypes.func,
    getUserColumns: PropTypes.func,
    getUserMenu: PropTypes.func,
    userMenuAction: PropTypes.func,
    appSettings: PropTypes.object,
    columnSizes: PropTypes.array,
    filter: PropTypes.number,
    addIsOpened: PropTypes.string,
    children: PropTypes.node
};

DetailsRow.defaultProps = {
    isEvoq: false
};

export default DetailsRow;