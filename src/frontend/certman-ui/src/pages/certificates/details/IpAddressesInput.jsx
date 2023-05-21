import { useState } from 'react';

function IpAddressesInput({ onIpAddressesChange, isValid }) {
    const [isInvalid, setIsInvalid] = useState(false);
    const [ipAddresses, setIpAddresses] = useState([]);
    const [currentIpAddress, setCurrentIpAddress] = useState('');

    const handleAdd = () => {
        const regex = new RegExp(
            /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/
        );

        if (currentIpAddress !== '' && !ipAddresses.includes(currentIpAddress)) {
            if (regex.test(currentIpAddress)) {
                const newDnsNames = [...ipAddresses, currentIpAddress];
                setIpAddresses(newDnsNames);
                setCurrentIpAddress('');
                onIpAddressesChange(newDnsNames);
                setIsInvalid(false);
            } else {
                setIsInvalid(true);
            }
        }

    };

    const handleRemove = (ipAddress) => {
        const newIpAddress = ipAddresses.filter(ip => ip !== ipAddress);
        setIpAddresses(newIpAddress);
        onIpAddressesChange(newIpAddress);
    };

    const handleInputChange = (e) => {
        setCurrentIpAddress(e.target.value);
    };

    return (
        <div className="field">
            <label className="label">IP Addresses:</label>
            {ipAddresses.map(name => (
                <div className="control" key={name}>
                    <div className="tags has-addons">
                        <span className="tag">{name}</span>
                        <a className="tag is-delete" onClick={() => handleRemove(name)} href="#"></a>
                    </div>
                </div>
            ))}
            <div className="field has-addons">
                <div className="control">
                    <input
                        className={`input is-small ${isInvalid ? 'is-danger' : ''}`}
                        type="text"
                        value={currentIpAddress}
                        onChange={handleInputChange}
                        placeholder="New IP Address"
                        maxLength={15}
                    />
                </div>
                <div className="control">
                    <button
                        className="button is-info is-small"
                        type="button"
                        onClick={handleAdd}
                    >
                        Add IP Address
                    </button>
                </div>
            </div>
        </div>
    );
}

export default IpAddressesInput;