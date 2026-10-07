import { Component } from "react";
import PropTypes from "prop-types";
import securityService from "../../services/securityService";
const logger = console;

class Sec extends Component {

    componentDidMount() {
	logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });

	logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    isVisible() {
        const { selectedPage, permission, onlySuperUsers, onlyForNotSuperUser } = this.props;

        const isSuperUser = securityService.isSuperUser();
		logger.error(`isVisible(): isSuperUser → ${isSuperUser}`, { "event.action": "isVisible" });
        if (onlyForNotSuperUser && isSuperUser) {
            return false;
        }

        if (isSuperUser) {
            return true;
        }

        if (onlySuperUsers) {
            return false;
        }

        return securityService.userHasPermission(permission, selectedPage);
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const isVisible = this.isVisible();
        if (!isVisible) {
            return null;
        }

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return this.props.children;
    }
}

Sec.propTypes = {
    className: PropTypes.string,
    children: PropTypes.node,
    onlySuperUsers: PropTypes.bool,
    onlyForNotSuperUser: PropTypes.bool,
    permission: PropTypes.string,
    selectedPage: PropTypes.object
};

export default Sec;