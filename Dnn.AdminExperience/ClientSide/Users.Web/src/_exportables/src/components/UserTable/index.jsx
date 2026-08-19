import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import HeaderRow from "./HeaderRow";
import DetailRow from "./DetailRow";
import CollapsibleSwitcher from "../common/CollapsibleSwitcher";
import CreateUserBox from "../CreateUserBox";
import UserSettings from "./UserSettings";
import EditProfile from "./EditProfile";
import UsersRoles from "./UsersRoles";
import styles from "./style.module.less";
import {sort} from "../../helpers";
import Localization from "localization";
import ColumnSizes from "./columnSizes";
const logger = console;
import {canManageRoles, canManageProfile, canViewSettings, canAddUser} from "../permissionHelpers.js";
import { GridCell } from "@dnnsoftware/dnn-react-common";

class UserTable extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            openId: "",
            renderIndex: -1
        };
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.collapse();
		logger.debug('<<< Exiting componentDidMount()');
    }

    unCollapse(id, index) {
		logger.debug(`>>> Entering unCollapse(id=${id},index=${index})`);
        this.setState({
            openId: id,
            renderIndex: index
        });
		logger.debug(`<<< Exiting unCollapse(id=${id},index=${index})`);
    }
    collapse() {
		logger.debug('>>> Entering collapse()');
        if (this.state.openId !== "") {
            this.setState({
                openId: "",
                renderIndex: -1
            });
			logger.debug('<<< Exiting collapse()');
        }
    }
    toggle(openId, index) {
		logger.debug(`>>> Entering toggle(openId=${openId},index=${index})`);
        if (openId !== "") {
            this.unCollapse(openId, index);
        } else {
            this.collapse();
			logger.debug(`<<< Exiting toggle(openId=${openId},index=${index})`);
        }
    }
    onAddUser() {
		logger.debug('>>> Entering onAddUser()');
        this.toggle(this.state.openId === "add" ? "" : "add", 0);
		logger.debug('<<< Exiting onAddUser()');
    }
    getChildren(user) {
		logger.debug(`>>> Entering getChildren(user=${user})`);
        let children = [];
        children = children.concat((this.props.getUserTabs && this.props.getUserTabs(user)) || []);
		logger.debug(`getChildren(user=${user}): children → ${children}`);
        if (canViewSettings(this.props.appSettings.applicationSettings.settings))
        {
            const userSettings = <UserSettings key={`usersettings-${user.userId}`} userId={user.userId} collapse={this.collapse.bind(this) } appSettings={this.props.appSettings}/>;
            children = children.concat([{
                index: 10,
                content: userSettings
            }]);
        }

        if (canManageRoles(this.props.appSettings.applicationSettings.settings, user))
        {
            children = children.concat([{
                index: 5,
                content: <UsersRoles key={`usersroles-${user.userId}`} userDetails={user} />
            }]);
        }

        if (canManageProfile(this.props.appSettings.applicationSettings.settings))
        {
            children = children.concat([{
                index: 15,
                content: <EditProfile key={`editprofile-${user.userId}`} userId={user.userId} />
            }]);
        }
		logger.debug(`<<< Exiting getChildren(user=${user})`);
        return sort(children, "index", "desc").map((child) => {
            return child.content;
        });
    }
    getHeaders()
    {
        let columnSizes =this.props.columnSizes!==undefined? this.props.columnSizes: ColumnSizes;
        let headers = [];
        if (this.props.appSettings.applicationSettings.settings.dataConsentActive) {
            headers = [{index: 3, size: columnSizes.find(x=>x.index===3).size, header: ""}];
        }
        headers = headers.concat([
            {index: 5, size: columnSizes.find(x=>x.index===5).size, header: Localization.get("Name.Header"), isSortable: true, columnName: "DisplayName"},
            {index: 10, size: columnSizes.find(x=>x.index===10).size, header: Localization.get("Email.Header"), isSortable: true, columnName: "Email"},
            {index: 15, size: columnSizes.find(x=>x.index===15).size, header: Localization.get("Created.Header"), isSortable: true, columnName: "Joined"},
            {index: 25, size: columnSizes.find(x=>x.index===25).size, header:""}]);
        if (this.props.getUserColumns !== undefined  && typeof this.props.getUserColumns ==="function") {
            let extraColumns = this.props.getUserColumns();
            if (extraColumns!==undefined && extraColumns.length>0)
            {
                headers = sort(extraColumns.map(column=>{
                    return {
                        index:column.index,
                        header:column.header,
                        size: columnSizes.find(x=>x.index===column.index).size
                    };
                }).concat(headers), "index");
            }
        }
        return headers;
    }
    
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        let i = 0;
        let opened = (this.state.openId === "add");
		logger.debug(`render(): opened → ${opened}`);
        const addIsOpened = opened && canAddUser(this.props.appSettings.applicationSettings.settings);
		logger.debug(`render(): addIsOpened → ${addIsOpened}`);
        const headers = this.getHeaders();
        const createUserBox = () => [<CreateUserBox key={`create-user-box-${i++}`} filter={props.filter} onCancel={this.collapse.bind(this) } appSettings={props.appSettings}/>];
		logger.debug('<<< Exiting render()');
        return (
            <GridCell className={styles.usersList}>
                <HeaderRow headers={headers} changeSortOrder={props.changeSortOrder} currentSortAscending={props.currentSortAscending} currentSortColumn={props.currentSortColumn} />
                <DetailRow
                    Collapse={this.toggle.bind(this) }
                    OpenCollapse={this.toggle.bind(this) }
                    currentIndex={this.state.renderIndex}
                    openId={this.state.openId }
                    key={"user-add"}
                    appSettings={props.appSettings}
                    columnSizes={props.columnSizes}
                    id={"add"}
                    addIsOpened={addIsOpened ? "add-opened" : "closed"}
                    getUserMenu={props.getUserMenu && props.getUserMenu.bind(this)}
                    userMenuAction={props.userMenuAction && props.userMenuAction.bind(this)}
                    filter={props.filter}>
                    <CollapsibleSwitcher>
                        {createUserBox()}
                    </CollapsibleSwitcher>
                </DetailRow>
                {
                    props.users && props.users.length>0 && props.users.map((user, index) => {
                        let id = "row-" + i++;
                        const children = this.getChildren(user);
                        return <DetailRow
                            user={user}
                            Collapse={this.collapse.bind(this) }
                            OpenCollapse={this.toggle.bind(this) }
                            currentIndex={this.state.renderIndex}
                            openId={this.state.openId }
                            key={"user-" + index}
                            getUserColumns={props.getUserColumns && props.getUserColumns.bind(this) }
                            getUserTabsIcons={props.getUserTabsIcons && props.getUserTabsIcons.bind(this) }
                            getUserMenu={props.getUserMenu && props.getUserMenu.bind(this)} 
                            userMenuAction={props.userMenuAction && props.userMenuAction.bind(this)}
                            appSettings={props.appSettings}
                            columnSizes={props.columnSizes}
                            id={id}
                            filter={props.filter}>
                            <CollapsibleSwitcher renderIndex={this.state.renderIndex}>
                                {children}
                            </CollapsibleSwitcher>
                        </DetailRow>;
                    }) 
                }
                {
                    props.users && props.users.length === 0 && <GridCell className="no-users">{Localization.get("noUsers")}</GridCell>
                }
            </GridCell>
        );
    }
}

UserTable.propTypes = {
    dispatch: PropTypes.func.isRequired,
    changeSortOrder: PropTypes.func,
    currentSortAscending: PropTypes.bool,
    currentSortColumn: PropTypes.string,
    getUserTabs: PropTypes.func,
    getUserTabsIcons: PropTypes.func,
    getUserColumns: PropTypes.func,
    getUserMenu: PropTypes.func,
    userMenuAction: PropTypes.func,
    appSettings: PropTypes.object,
    columnSizes: PropTypes.array,
    filter: PropTypes.number,
    users: PropTypes.array
};
function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        users: state.users.users
    };
}

export default connect(mapStateToProps, null, null, { forwardRef: true, })(UserTable);