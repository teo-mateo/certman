import { useCallback } from 'react';

function DownloadButton({ caCertId, certId, fileName, apiMethod }) {
    const downloadFile = useCallback(async () => {
        const fileBlob = await apiMethod(caCertId, certId);
        const url = window.URL.createObjectURL(fileBlob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', fileName);
        document.body.appendChild(link);
        link.click();
        link.remove();
    }, [certId, fileName, apiMethod]);

    return (
        <button className="button is-ghost" onClick={downloadFile}>
            <span className="icon">
                <i className="fa fa-download"></i>
            </span>
            <span><code>{fileName}</code></span>
        </button>
    );
}

export default DownloadButton;