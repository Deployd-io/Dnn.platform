import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import { Collapsible as Collapse, Switch, DropdownWithError, RadioButtons, Button, PersonaBarPageHeader, PersonaBarPageBody, GridCell, GridSystem, SingleLineInputWithError, MultiLineInputWithError } from "@dnnsoftware/dnn-react-common";
import { CommonPortalListActions } from "dnn-sites-common-actions";
import Localization from "localization";
import "./style.less";
const logger = console;

const emptyNewPortal = {
    SiteTemplate: "",
    SiteName: "",
    SiteAlias: "",
    SiteDescription: "",
    SiteKeywords: "",
    IsChildSite: false,
    HomeDirectory: "Portals/[PortalID]",
    UseCurrentUserAsAdmin: true,
    Firstname: "",
    Lastname: "",
    Username: "",
    Email: "",
    Password: "",
    PasswordConfirm: ""
};
function extractDomain(url) {
	logger.error(`>>> Entering extractDomain(url=${url})`, { "event.action": "extractDomain", "logrite.url": url });
    let domain, port;
    //find & remove protocol (http, ftp, etc.) and get domain
    if (url.indexOf("://") > -1) {
        domain = url.split("/")[2];
		logger.error(`extractDomain(url=${url}): domain → ${domain}`, { "event.action": "extractDomain", "logrite.url": url });
    }
    else {
        domain = url.split("/")[0];
    }
    [domain, port] = domain.split(":");
	logger.warn(`<<< Exiting extractDomain(url=${url})`, { "event.action": "extractDomain", "logrite.url": url });
    return port !== undefined && port !== 80 && port !== 443 ? `${domain}:${port}` : domain;
}

function validateEmail(email) {
    let re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}

function getUniqueId() {
	logger.error('>>> Entering getUniqueId()', { "event.action": "getUniqueId" });
	logger.warn('<<< Exiting getUniqueId()', { "event.action": "getUniqueId" });
    return Math.random() * Date.now();
}

class CreatePortal extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            defaultTemplate: "",
            newPortal: Object.assign({}, emptyNewPortal),
            error: {
                SiteName: true,
                SiteAlias: true,
                Firstname: false,
                Lastname: false,
                Username: false,
                Email: false,
                Password: false,
                PasswordConfirm: false
            },
            triedToSave: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    resetNewPortal() {
		logger.error('>>> Entering resetNewPortal()', { "event.action": "resetNewPortal" });
        this.setState({
            newPortal: Object.assign({}, emptyNewPortal)
        });
		logger.warn('<<< Exiting resetNewPortal()', { "event.action": "resetNewPortal" });
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const { props, state } = this;
        props.dispatch(CommonPortalListActions.getPortalTemplates((data) => {
            let {newPortal} = state;
			logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
            newPortal.SiteTemplate = data.Results.DefaultTemplate;
            this.setState({
                defaultTemplate: data.Results.DefaultTemplate,
                newPortal
            });
        }));
    }
    resolveSiteUrl(isChildSite) {
		logger.error(`>>> Entering resolveSiteUrl(isChildSite=${isChildSite})`, { "event.action": "resolveSiteUrl", "logrite.isChildSite": isChildSite });
        let rootDomain = extractDomain(window.location.href);
        if (isChildSite) {
            let { newPortal, error } = this.state;
            if (newPortal.SiteAlias !== "" && newPortal.SiteAlias.indexOf(rootDomain) === -1) {
                newPortal.SiteAlias = rootDomain + "/" + newPortal.SiteAlias;
            } else {
                newPortal.SiteAlias = rootDomain + "/" + newPortal.SiteName.replace(/[^a-zA-Z0-9-_]/g, "");
            }
            error.SiteAlias = this.resolveSiteAliasError(newPortal.SiteAlias, isChildSite);
            this.setState({ newPortal, error });
        } else {
            let { newPortal } = this.state;
            newPortal.SiteAlias = newPortal.SiteAlias.replace(rootDomain + "/", "");
			logger.debug(`resolveSiteUrl(isChildSite=${isChildSite}): newPortal.SiteAlias → ${newPortal.SiteAlias}`, { "event.action": "resolveSiteUrl", "logrite.isChildSite": isChildSite });
            this.setState({ newPortal });
			logger.warn(`<<< Exiting resolveSiteUrl(isChildSite=${isChildSite})`, { "event.action": "resolveSiteUrl", "logrite.isChildSite": isChildSite });
        }
    }

    resolveSiteAliasError(value, isChildSite) {
		logger.error(`>>> Entering resolveSiteAliasError(value=${value},isChildSite=${isChildSite})`, { "event.action": "resolveSiteAliasError", "logrite.value": value, "logrite.isChildSite": isChildSite });
        let rootDomain = extractDomain(window.location.href);
        let subUrl = value.replace(rootDomain + "/", "");
		logger.error(`resolveSiteAliasError(value=${value},isChildSite=${isChildSite}): rootDomain → ${rootDomain}`, { "event.action": "resolveSiteAliasError", "logrite.value": value, "logrite.isChildSite": isChildSite });
        if (!isChildSite) {
            let regex = /[^\/a-z0-9-.:]/i;
            return value === "" || regex.test(value) || value.indexOf(" ") > 0;
        } else {
            let regex = /[^\/a-z0-9_-]/i;
            if (((regex.test(subUrl) && subUrl !== "") || (value.indexOf(rootDomain + "/")) === -1) || subUrl === "" || value.indexOf(" ") > 0) {
                return true;
            }
			logger.warn(`<<< Exiting resolveSiteAliasError(value=${value},isChildSite=${isChildSite})`, { "event.action": "resolveSiteAliasError", "logrite.value": value, "logrite.isChildSite": isChildSite });
            return false;
        }
    }

    resolvePasswordError(value) {
		logger.error(`>>> Entering resolvePasswordError(value=${value})`, { "event.action": "resolvePasswordError", "logrite.value": value });
        if (value !== this.state.newPortal.Password || value === "" || value.length < 7) {
            return true;
        }
		logger.warn(`<<< Exiting resolvePasswordError(value=${value})`, { "event.action": "resolvePasswordError", "logrite.value": value });
        return false;
    }
    resolveEmailError(value) {
		logger.error(`>>> Entering resolveEmailError(value=${value})`, { "event.action": "resolveEmailError", "logrite.value": value });
		logger.warn(`<<< Exiting resolveEmailError(value=${value})`, { "event.action": "resolveEmailError", "logrite.value": value });
        return !validateEmail(value);
    }
    onChange(key, event) {
        const value = typeof event === "object" ? event.target.value : event;
        let {newPortal, error} = this.state;
        switch (key) {
            case "IsChildSite"://Convert radio button's return of string to boolean.
                newPortal[key] = (value === "true");
                this.resolveSiteUrl((value === "true"));
                break;
            case "PasswordConfirm":
                newPortal[key] = value;
                error[key] = this.resolvePasswordError(value);
                break;
            case "Email":
                newPortal[key] = value;
                error[key] = this.resolveEmailError(value);
                break;
            case "Password":
            case "Firstname":
            case "Lastname":
            case "Username":
            case "SiteName":
                newPortal[key] = value;
                error[key] = (value === "");
                break;
            case "SiteAlias":
                newPortal[key] = value;
                error[key] = this.resolveSiteAliasError(value, newPortal.IsChildSite);
                break;
            case "UseCurrentUserAsAdmin":
                newPortal[key] = value;
                if ((value === true)) {
                    error.Firstname = false;
                    error.Lastname = false;
                    error.Username = false;
                    error.Email = false;
                    error.Password = false;
                    error.PasswordConfirm = false;
                } else {
                    error.Firstname = (newPortal.Firstname === "");
                    error.Lastname = (newPortal.Lastname === "");
                    error.Username = (newPortal.Username === "");
                    error.Email = this.resolveEmailError(newPortal.Email);
                    error.Password = (newPortal.Password === "");
                    error.PasswordConfirm = this.resolvePasswordError(newPortal.PasswordConfirm);
                }
                break;
            default:
                newPortal[key] = value;
                break;
        }
        this.setState({
            newPortal,
            error
        });
    }
    onSelect(option) {
        let {newPortal} = this.state;
        newPortal.SiteTemplate = option.value;
        this.setState({
            newPortal
        });
    }
    checkForError(newPortal, key) {
		logger.error(`>>> Entering checkForError(newPortal=${newPortal},key=${key})`, { "event.action": "checkForError", "logrite.newPortal": newPortal, "logrite.key": key });
        if (newPortal[key] === "") {
			logger.warn(`<<< Exiting checkForError(newPortal=${newPortal},key=${key})`, { "event.action": "checkForError", "logrite.newPortal": newPortal, "logrite.key": key });
            return true;
        }
    }
    setCreatingPortal(creatingPortal) {
		logger.error(`>>> Entering setCreatingPortal(creatingPortal=${creatingPortal})`, { "event.action": "setCreatingPortal", "logrite.creatingPortal": creatingPortal });
        this.setState({
            creatingPortal
        });
		logger.warn(`<<< Exiting setCreatingPortal(creatingPortal=${creatingPortal})`, { "event.action": "setCreatingPortal", "logrite.creatingPortal": creatingPortal });
    }
    createPortal() {
		logger.error('>>> Entering createPortal()', { "event.action": "createPortal" });
        const { props, state } = this;
        let triedToSave = true;
        this.setState({
            triedToSave
        }, () => {
            let withError = false;
            Object.keys(state.error).forEach((errorKey) => {
                if (state.error[errorKey]) {
                    withError = true;
                }
            });
            if (withError) {
                return;
            }
            this.setCreatingPortal(true);
            props.dispatch(CommonPortalListActions.createPortal(state.newPortal, () => {
                this.setCreatingPortal(false);
                this.resetNewPortal();
                props.onCancel();
            }, () => {
                //active the button after error message disappear
                setTimeout(() => {
                    this.setCreatingPortal(false);
                }, 5200);
            }));
        });
		logger.warn('<<< Exiting createPortal()', { "event.action": "createPortal" });
    }

    onCancel(event) {
		logger.error(`>>> Entering onCancel(event=${event})`, { "event.action": "onCancel", "logrite.event": event });
        if (event) {
            event.preventDefault();
        }
        this.props.onCancel();
        this.resetNewPortal();
		logger.warn(`<<< Exiting onCancel(event=${event})`, { "event.action": "onCancel", "logrite.event": event });
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;

        const templateOptions = props.portalTemplates.map(template => {
            return {
                label: template.Name,
                value: template.Value
            };
        });

        return (
            <div className="create-portal">
                <PersonaBarPageHeader title={Localization.get("AddNewSite.Header")} />
                <PersonaBarPageBody backToLinkProps={{
                    text: Localization.get("BackToSites"),
                    onClick: props.onCancel.bind(this)
                }}>
                    <GridCell className="create-site-container">
                        <GridCell>
                            <SingleLineInputWithError
                                label={Localization.get("Title.Label") + "*"}
                                inputId="add-new-site-title"
                                value={state.newPortal.SiteName}
                                errorMessage={Localization.get("SiteTitleRequired.Error")}
                                onChange={this.onChange.bind(this, "SiteName")}
                                error={state.error.SiteName && state.triedToSave}
                            />
                        </GridCell>
                        <GridCell>
                            <MultiLineInputWithError
                                label={Localization.get("Description")}
                                inputId="add-new-site-description"
                                value={state.newPortal.SiteDescription}
                                className="portal-description"
                                onChange={this.onChange.bind(this, "SiteDescription")}
                                error={false}
                            />
                            <hr />
                        </GridCell>
                        <GridCell className="site-thumbnails-container">
                            <DropdownWithError
                                options={templateOptions}
                                label={Localization.get("SiteTemplate.Label")}
                                value={state.newPortal.SiteTemplate}
                                defaultDropdownValue={state.defaultTemplate}
                                onSelect={this.onSelect.bind(this)}
                            />
                            <hr />
                        </GridCell>
                        <GridCell className="site-type-container">
                            <GridCell columnSize={55}>
                                <RadioButtons
                                    label={Localization.get("SiteType.Label")}
                                    onChange={this.onChange.bind(this, "IsChildSite")}
                                    buttonGroup="siteType"
                                    value={state.newPortal.IsChildSite}
                                    options={[
                                        {
                                            label: Localization.get("Domain"),
                                            value: false
                                        },
                                        {
                                            label: Localization.get("Directory"),
                                            value: true
                                        }
                                    ]} />
                            </GridCell>
                            <GridCell columnSize={45}>
                                <SingleLineInputWithError
                                    label={Localization.get("Directory")}
                                    className="home-directory"
                                    inputId="home-directory"
                                    value={state.newPortal.HomeDirectory}
                                    onChange={this.onChange.bind(this, "HomeDirectory")}
                                    labelType="inline"
                                    error={false}
                                />
                            </GridCell>
                            <GridCell>
                                <SingleLineInputWithError
                                    label={Localization.get("SiteUrl.Label")}
                                    inputId="site-url"
                                    value={state.newPortal.SiteAlias}
                                    onChange={this.onChange.bind(this, "SiteAlias")}
                                    errorMessage={Localization.get("SiteAliasRequired.Error")}
                                    error={state.error.SiteAlias && state.triedToSave}
                                />
                            </GridCell>
                        </GridCell>
                        <GridCell className="user-as-admin">
                            <Switch
                                label={Localization.get("AssignCurrentUserAsAdmin.Label")}
                                onText={Localization.get("SwitchOn")}
                                offText={Localization.get("SwitchOff")}
                                value={state.newPortal.UseCurrentUserAsAdmin}
                                onChange={this.onChange.bind(this, "UseCurrentUserAsAdmin")}
                            />
                            <Collapse className="user-as-admin-collapse" isOpened={!this.state.newPortal.UseCurrentUserAsAdmin}>
                                <GridSystem className="with-right-border top-half">
                                    <GridCell>
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminUserName.Label")}
                                            inputId="admin-user-name"
                                            value={state.newPortal.Username}
                                            onChange={this.onChange.bind(this, "Username")}
                                            errorMessage={Localization.get("UsernameRequired.Error")}
                                            error={state.error.Username && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            autoComplete={getUniqueId()}
                                        />
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminFirstName.Label")}
                                            inputId="admin-first-name"
                                            value={state.newPortal.Firstname}
                                            onChange={this.onChange.bind(this, "Firstname")}
                                            errorMessage={Localization.get("FirstNameRequired.Error")}
                                            error={state.error.Firstname && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            autoComplete={getUniqueId()}
                                        />
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminLastName.Label")}
                                            inputId="admin-last-name"
                                            value={state.newPortal.Lastname}
                                            onChange={this.onChange.bind(this, "Lastname")}
                                            errorMessage={Localization.get("LastNameRequired.Error")}
                                            error={state.error.Lastname && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            autoComplete={getUniqueId()}
                                        />
                                    </GridCell>
                                    <GridCell>
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminEmail.Label")}
                                            inputId="admin-email"
                                            value={state.newPortal.Email}
                                            onChange={this.onChange.bind(this, "Email")}
                                            errorMessage={Localization.get("EmailRequired.Error")}
                                            error={state.error.Email && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            autoComplete={getUniqueId()}
                                        />
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminPassword.Label")}
                                            inputId="admin-password"
                                            value={state.newPortal.Password}
                                            type="password"
                                            onChange={this.onChange.bind(this, "Password")}
                                            errorMessage={Localization.get("PasswordRequired.Error")}
                                            error={state.error.Password && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            autoComplete={getUniqueId()}
                                        />
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminPasswordConfirm.Label")}
                                            inputId="admin-password-confirm"
                                            type="password"
                                            value={state.newPortal.PasswordConfirm}
                                            errorMessage={Localization.get("PasswordConfirmRequired.Error")}
                                            onChange={this.onChange.bind(this, "PasswordConfirm")}
                                            error={state.error.PasswordConfirm && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            autoComplete={getUniqueId()}
                                        />
                                    </GridCell>
                                </GridSystem>
                                <div style={{clear:"both"}}></div>
                            </Collapse>
                        </GridCell>
                        <GridCell className="site-action-buttons">
                            <Button type="neutral" onClick={this.onCancel.bind(this)}>{Localization.get("cmdCancel")}</Button>
                            <Button type="primary" disabled={this.state.creatingPortal} onClick={this.createPortal.bind(this)}>{Localization.get("cmdCreateSite")}</Button>
                        </GridCell>
                    </GridCell>
                </PersonaBarPageBody>
            </div>
        );
    }
}

CreatePortal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    portalTemplates: PropTypes.array,
};


function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        portalTemplates: state.exportPortal.templates
    };
}


export default connect(mapStateToProps)(CreatePortal);