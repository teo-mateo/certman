// CreateCertModal.jsx
import React, { useState } from 'react';
import Modal from 'react-modal';
import apiService from './../../services/apiService';

Modal.setAppElement('#root'); // replace '#root' with your app element id

const CreateCertModal = ({ onCertCreated }) => {
    const [modalIsOpen, setModalIsOpen] = useState(false);
    const [newCertName, setNewCertName] = useState('');

    const openModal = () => {
        setModalIsOpen(true);
    }

    const closeModal = () => {
        setModalIsOpen(false);
    }

    const customStyles = {
        content: {
            width: '800px',
            height: '600px',
            margin: 'auto'
        },
        overlay: {
            backgroundColor: 'rgba(0,0,0,0.5)'
        }
    };

    const handleCreateCert = async () => {
        const payload = {
            name: newCertName,
        };

        await apiService.createCACert(payload)
            .then(data => {
                onCertCreated(data);
                closeModal();
            })
            .catch(error => {
                console.error('There was a problem creating the new certificate:', error);
            });
    }

    return (
        <div>
            <button className="button is-primary is-small" onClick={openModal}>Create New Certificate</button>
            <Modal
                isOpen={modalIsOpen}
                onRequestClose={closeModal}
                contentLabel="New Certificate"
                style={customStyles}
            >
                <div className="box">
                    <h2 className="title is-4">Create New Certificate</h2>
                    <div className="field">
                        <label className="label">Certificate Name</label>
                        <div className="control">
                            <input
                                className="input"
                                type="text"
                                value={newCertName}
                                onChange={(e) => setNewCertName(e.target.value)}
                                placeholder="Enter Certificate Name"
                            />
                        </div>
                    </div>
                    <div className="field is-grouped">
                        <div className="control">
                            <button className="button is-link is-small" onClick={handleCreateCert}>Submit</button>
                        </div>
                        <div className="control">
                            <button className="button is-link is-light is-small" onClick={closeModal}>Cancel</button>
                        </div>
                    </div>
                </div>
            </Modal>
        </div>
    );
};

export default CreateCertModal;
