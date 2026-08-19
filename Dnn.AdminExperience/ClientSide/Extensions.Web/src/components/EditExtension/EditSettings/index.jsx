import React, { Component } from "react";
import AuthenticationSystemSettings from "./AuthenticationSystemSettings";
import ModuleSettings from "./ModuleSettings";
import ContainerSettings from "../CustomSettings/Container";
import JavaScriptLibrary from "../CustomSettings/JavascriptLibrary";
import SkinObject from "../CustomSettings/SkinObject";
import WrapWithContainer from "./WrapWithContainer";
import CoreLanguagePack from "../CustomSettings/CoreLanguagePack";
import ExtensionLanguagePack from "../CustomSettings/ExtensionLanguagePack";
import IFrameHandler from "./IFrameHandler";
const logger = console;

function checkIfKnownAuthenticationProvider(name) {
	logger.debug(`>>> Entering checkIfKnownAuthenticationProvider(name=${name})`);
    switch (name) {
        case "DNN_FacebookAuthentication":
        case "DNN_GoogleAuthentication":
        case "DNN_LiveAuthentication":
        case "DNN_TwitterAuthentication":
            return true;
        default:
			logger.debug(`<<< Exiting checkIfKnownAuthenticationProvider(name=${name})`);
            return false;
    }
}

class EditSettings extends Component {
    getSettingsEditor(props) {
		logger.debug(`>>> Entering getSettingsEditor(props=${props})`);
        switch (props.type) {
            case "Module":
                return <ModuleSettings {...props} />;
            case "Auth_System":
                if (checkIfKnownAuthenticationProvider(props.extensionBeingEdited.name.value)) {
                    return <AuthenticationSystemSettings {...props} />;
                } else {
                    return <IFrameHandler {...props} />;
                }
            case "Container":
            case "Skin":
                return <WrapWithContainer>
                    <ContainerSettings { ...props} disabled={true} />
                </WrapWithContainer>;
            case "JavaScript_Library":
                return <WrapWithContainer>
                    <JavaScriptLibrary { ...props} disabled={true} />
                </WrapWithContainer>;
            case "SkinObject":
                return <WrapWithContainer>
                    <SkinObject { ...props} disabled={true} />
                </WrapWithContainer>;
            case "CoreLanguagePack":
                return <WrapWithContainer>
                    <CoreLanguagePack { ...props} disabled={true} />
                </WrapWithContainer>;
            case "ExtensionLanguagePack":
                return <WrapWithContainer>
                    <ExtensionLanguagePack { ...props} disabled={true} />
                </WrapWithContainer>;
            default:
				logger.debug(`<<< Exiting getSettingsEditor(props=${props})`);
                return <IFrameHandler {...props} />;
        }
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;

		logger.debug('<<< Exiting render()');
        return this.getSettingsEditor(props);
    }
}

export default EditSettings;