import React, { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";
import { Collapsible, GridCell, IconButton }  from "@dnnsoftware/dnn-react-common";
import resx from "resources";
import util from "utils";
import CheckIcon from "../../../img/common/checkmark.svg?react";
const logger = console;

let canEdit = false;

const roleLabelStyle = {
    wordWrap: "break-word"
};

class RoleRow extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.handleClick = this.handleClick.bind(this);
        canEdit = util.settings.isHost || util.settings.isAdmin || util.settings.permissions.EDIT;
		logger.debug('<<< Exiting constructor()');
    }
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });

		logger.debug(`componentDidMount(): opened → ${opened}`);
        document.addEventListener("click", this.handleClick);
        this._isMounted = true;
		logger.debug(`componentDidMount(): this._isMounted → ${this._isMounted}`);
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
		logger.debug(`componentWillUnmount(): this._isMounted → ${this._isMounted}`);
		logger.debug('<<< Exiting componentWillUnmount()');
    }

    handleClick(event) {
		logger.debug(`>>> Entering handleClick(event=${event})`);
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }
        if (! this.node.contains(event.target) && (typeof event.target.className === "string" && event.target.className.indexOf("do-not-close") === -1)
            && !(event.target.id === "confirmbtn" || event.target.id === "cancelbtn") && this.props.openId !== "add") {

            this.timeout = 475;
            if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
                this.props.Collapse();
            }
        } else {

            this.timeout = 0;
			logger.debug(`<<< Exiting handleClick(event=${event})`);
        }
    }
    toggleEditRole() {
		logger.debug('>>> Entering toggleEditRole()');
        if ((this.props.openId !== "" && this.props.id === this.props.openId) && this.props.currentIndex === 1) {
            this.props.Collapse();
        } else {
            this.props.OpenCollapseEditRoles(this.props.id);
			logger.debug('<<< Exiting toggleEditRole()');
        }
    }
    toggleUsers() {
		logger.debug('>>> Entering toggleUsers()');
        if ((this.props.openId !== "" && this.props.id === this.props.openId) && this.props.currentIndex === 0) {
            this.props.Collapse();
        } else {
            this.props.OpenCollapseUsers(this.props.id);
			logger.debug('<<< Exiting toggleUsers()');
        }
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
		logger.debug(`render(): opened → ${opened}`);
        let uniqueId = "roleRow-" + Math.random() + Date.now();
		logger.debug(`render(): uniqueId → ${uniqueId}`);

        if (props.visible) {
            return (
                <div ref={node => this.node = node} className={"collapsible-component1 " + opened} id={uniqueId}>
                    {!props.addIsClosed && <div className={"collapsible-header1 " + !opened}>
                        <GridCell style={roleLabelStyle} title={props.roleName} columnSize={40} className="ellipsis">
                            {props.roleName}</GridCell>
                        <GridCell columnSize={20} >
                            {props.groupName}</GridCell>
                        <GridCell columnSize={15} >
                            {props.userCount}</GridCell>
                        <GridCell columnSize={15} >
                            {props.auto ? <IconButton className="icon-flat" customIcon={CheckIcon} /> : <div>&nbsp; </div>} </GridCell>
                        {canEdit &&
                            <GridCell columnSize={10} >
                                {props.id !== "add" && props.roleIsApproved &&
                                    <IconButton type="user"
                                        className={"edit-icon " + !(opened && props.currentIndex === 0) }
                                        onClick={this.toggleUsers.bind(this) }
                                        title={resx.get("UsersInRole") }/>
                                }
                                <IconButton type="edit"
                                    className={"edit-icon " + !(opened && props.currentIndex === 1) }
                                    onClick={this.toggleEditRole.bind(this) }
                                    title={resx.get("EditRole") }/>
                            </GridCell>
                        }
                    </div>
                    }
                    <Collapsible accordion={true} isOpened={opened} className="role-row-collapsible">
                        {opened && props.children}
                    </Collapsible>
                </div>
            );
        } else {
			logger.debug('<<< Exiting render()');
            return <div />;
        }
    }
}
RoleRow.propTypes = {
    roleName: PropTypes.string,
    groupName: PropTypes.string,
    userCount: PropTypes.number,
    auto: PropTypes.bool,
    OpenCollapseUsers: PropTypes.func,
    OpenCollapseEditRoles: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    currentIndex: PropTypes.number,
    visible: PropTypes.bool,
    roleIsApproved: PropTypes.bool,
    addIsClosed: PropTypes.bool,
    children: PropTypes.node
};
RoleRow.defaultProps = {
    collapsed: true,
    visible: true,
    roleIsApproved: false
};

export default (RoleRow);