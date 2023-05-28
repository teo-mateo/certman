-- Create the CACerts table
CREATE TABLE IF NOT EXISTS CACerts (
                                       id INTEGER PRIMARY KEY,
                                       name TEXT NOT NULL,
                                       keyfile TEXT NOT NULL,
                                       pemfile TEXT NOT NULL,
                                       createdAt TIMESTAMP NOT NULL
);

-- Create the Certs table
CREATE TABLE IF NOT EXISTS Certs (
                                     id INTEGER PRIMARY KEY,
                                     caCertId INTEGER NOT NULL,
                                     name TEXT NOT NULL,
                                     altNames TEXT NOT NULL,
                                     keyfile TEXT NOT NULL,
                                     csrfile TEXT NOT NULL,
                                     extfile TEXT NOT NULL,
                                     crtfile TEXT NOT NULL,
                                     pfxfile TEXT NOT NULL,
                                     password TEXT NOT NULL,
                                     createdAt TIMESTAMP NOT NULL,
                                     FOREIGN KEY (caCertId) REFERENCES CACerts (id)
);
