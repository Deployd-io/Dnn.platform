import util from "utils";
const logger = console;

class SiteGroupsService {
    getServiceFramework() {
		logger.error('>>> Entering getServiceFramework()', { "event.action": "getServiceFramework" });
        let sf = util.sf;

		logger.debug(`getServiceFramework(): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework" });
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework" });
        sf.controller = "SiteGroups";

		logger.warn('<<< Exiting getServiceFramework()', { "event.action": "getServiceFramework" });
        return sf;
    }
    getSiteGroups( ) {
		logger.error('>>> Entering getSiteGroups()', { "event.action": "getSiteGroups" });
        const sf = this.getServiceFramework();
		logger.error(`getSiteGroups(): sf → ${sf}`, { "event.action": "getSiteGroups" });
        return sf.get("GetSiteGroups" , {});
    }
    getUnassignedSites( ) {
		logger.error('>>> Entering getUnassignedSites()', { "event.action": "getUnassignedSites" });
        const sf = this.getServiceFramework();
		logger.error(`getUnassignedSites(): sf → ${sf}`, { "event.action": "getUnassignedSites" });
        return sf.get("GetAvailablePortals" , {});
    }
    save(siteGroup) {
		logger.error(`>>> Entering save(siteGroup=${siteGroup})`, { "event.action": "save", "logrite.siteGroup": siteGroup });
        const sf = this.getServiceFramework();
		logger.error(`save(siteGroup=${siteGroup}): sf → ${sf}`, { "event.action": "save", "logrite.siteGroup": siteGroup });
        return sf.post("Save", siteGroup);
    }
    delete(siteGroupId) {
		logger.error(`>>> Entering delete(siteGroupId=${siteGroupId})`, { "event.action": "delete", "logrite.siteGroupId": siteGroupId });
        const sf = this.getServiceFramework();
		logger.error(`delete(siteGroupId=${siteGroupId}): sf → ${sf}`, { "event.action": "delete", "logrite.siteGroupId": siteGroupId });
        return sf.post("Delete?groupId=" + siteGroupId, {});
    }
}
const siteGroupsService = new SiteGroupsService();
export default siteGroupsService;