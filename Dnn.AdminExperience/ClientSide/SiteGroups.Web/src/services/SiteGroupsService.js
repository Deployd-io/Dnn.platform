import util from "utils";
const logger = console;

class SiteGroupsService {
    getServiceFramework() {
		logger.debug('>>> Entering getServiceFramework()');
        let sf = util.sf;

        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(): sf.moduleRoot → ${sf.moduleRoot}`);
        sf.controller = "SiteGroups";
		logger.debug(`getServiceFramework(): sf.controller → ${sf.controller}`);

		logger.debug('<<< Exiting getServiceFramework()');
        return sf;
    }
    getSiteGroups( ) {
		logger.debug('>>> Entering getSiteGroups()');
        const sf = this.getServiceFramework();
		logger.debug(`getSiteGroups(): sf → ${sf}`);
		logger.debug('<<< Exiting getSiteGroups()');
        return sf.get("GetSiteGroups" , {});
    }
    getUnassignedSites( ) {
		logger.debug('>>> Entering getUnassignedSites()');
        const sf = this.getServiceFramework();
		logger.debug(`getUnassignedSites(): sf → ${sf}`);
		logger.debug('<<< Exiting getUnassignedSites()');
        return sf.get("GetAvailablePortals" , {});
    }
    save(siteGroup) {
		logger.debug(`>>> Entering save(siteGroup=${siteGroup})`);
        const sf = this.getServiceFramework();
		logger.debug(`save(siteGroup=${siteGroup}): sf → ${sf}`);
		logger.debug(`<<< Exiting save(siteGroup=${siteGroup})`);
        return sf.post("Save", siteGroup);
    }
    delete(siteGroupId) {
		logger.debug(`>>> Entering delete(siteGroupId=${siteGroupId})`);
        const sf = this.getServiceFramework();
		logger.debug(`delete(siteGroupId=${siteGroupId}): sf → ${sf}`);
		logger.debug(`<<< Exiting delete(siteGroupId=${siteGroupId})`);
        return sf.post("Delete?groupId=" + siteGroupId, {});
    }
}
const siteGroupsService = new SiteGroupsService();
export default siteGroupsService;