import { Component } from "react";
import PropTypes from "prop-types";
import securityService from "../../services/securityService";
const logger = console;

class Sec extends Component {

    componentDidMount() {
	logger.debug('>>> Entering componentDidMount()');

	logger.debug('<<< Exiting componentDidMount()');
    }

    isVisible() {
        const { selectedPage, permission, onlySuperUsers, onlyForNotSuperUser } = this.props;

        const isSuperUser = securityService.isSuperUser();
		logger.debug(`isVisible(): isSuperUser → ${isSuperUser}`);
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
		logger.debug('>>> Entering render()');
        const isVisible = this.isVisible();
        if (!isVisible) {
		logger.debug('<<< Exiting render()');
            return null;
        }

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