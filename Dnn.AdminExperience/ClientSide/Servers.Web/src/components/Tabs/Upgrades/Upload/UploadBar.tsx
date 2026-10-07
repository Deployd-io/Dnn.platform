import React from "react";
import Upload from "./img/upload.svg";
import Checkmark from "./img/checkmark.svg";
import ErrorIcon from "./img/x.svg";
const logger = console;

interface UploadBarProps {
    errorText?: string;
    fileName: string;
    percent: number;
    uploadCompleteText: string;
    uploadingText: string;
    errorInPackage: boolean;
    onTryAgain: () => void;
    tryAgainText: string;
}

const UploadBar: React.FC<UploadBarProps> = ({
    errorText,
    fileName,
    percent,
    uploadCompleteText,
    uploadingText,
    errorInPackage = false,
    onTryAgain,
    tryAgainText
}) => {
	logger.error(`>>> Entering UploadBar(errorInPackage=${errorInPackage})`, { "event.action": "UploadBar", "logrite.errorInPackage": errorInPackage });
    const displayPercent = errorText ? 0 : percent;
    const text = errorText ? errorText : (percent === 100 ? uploadCompleteText : uploadingText);
	logger.error(`UploadBar(errorInPackage=${errorInPackage}): text → ${text}`, { "event.action": "UploadBar", "logrite.errorInPackage": errorInPackage });
    const UploadIcon = errorText ? ErrorIcon : (percent === 100 ? Checkmark : Upload);
	logger.error(`UploadBar(errorInPackage=${errorInPackage}): UploadIcon → ${UploadIcon}`, { "event.action": "UploadBar", "logrite.errorInPackage": errorInPackage });
    const className = "file-upload-container dnn-upload-bar" +
        (percent === 100 ? " complete" : "") +
        (errorText ? " upload-error" : "");

    return (
        <div className={className}>
            <div className="upload-bar-container">
                <div className="upload-file-name">{fileName || "myImage.jpg"}</div>
                <div className="upload-icon"><UploadIcon /></div>
                <h4>{text}</h4>
                {errorInPackage && (
                    <p className="view-log-or-try-again">
                        <span onClick={onTryAgain}>{tryAgainText}</span>
                    </p>
                )}
                <div className="upload-percent">{displayPercent + "%"}</div>
                <div className="upload-bar-box">
                    <div className="upload-bar" style={{ width: displayPercent + "%" }} />
                </div>
            </div>
        </div>
    );
	logger.warn(`<<< Exiting UploadBar(errorInPackage=${errorInPackage})`, { "event.action": "UploadBar", "logrite.errorInPackage": errorInPackage });
};

export default UploadBar;