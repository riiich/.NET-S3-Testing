import { useState } from "react";
import { createUser, getUserFiles, loginUser, uploadFile } from "../lib/api";
import type { S3Item, User } from "../lib/api";

export default function FileUploader(): React.JSX.Element {
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [name, setName] = useState<string>("");
    const [email, setEmail] = useState<string>("");
    const [currentUser, setCurrentUser] = useState<User | null>(null);
    const [files, setFiles] = useState<S3Item[]>([]);
    const [status, setStatus] = useState<string>("");

    const acceptedFilesTypes: string[] = [
        "application/pdf",
        "application/rtf",
        "text/plain",
        "image/png",
        "image/jpeg",
        "image/gif",
    ];

    const loadFiles = async (userId: number): Promise<void> => {
        const userFiles: S3Item[] = await getUserFiles(userId);
        setFiles(userFiles);
    };

    const handleFile = (event: React.ChangeEvent<HTMLInputElement>): void => {
        const currentFile: File | undefined = event.target.files?.[0];

        setSelectedFile(currentFile ?? null);
    };

    const handleCreateUser = async (): Promise<void> => {
        try {
            const user: User = await createUser(name, email);
            setCurrentUser(user);
            setStatus(`Created and signed in as ${user.name}.`);
            await loadFiles(user.id);
        } catch (error) {
            setStatus(error instanceof Error ? error.message : "There was an error creating the user.");
        }
    };

    const handleLogin = async (): Promise<void> => {
        try {
            const user: User = await loginUser(name, email);
            setCurrentUser(user);
            setStatus(`Signed in as ${user.name}.`);
            await loadFiles(user.id);
        } catch (error) {
            setCurrentUser(null);
            setFiles([]);
            setStatus(error instanceof Error ? error.message : "There was an error signing in.");
        }
    };

    const handleSignOut = (): void => {
        setCurrentUser(null);
        setSelectedFile(null);
        setFiles([]);
        setStatus("Signed out.");
    };

    const handleUpload = async (): Promise<void> => {
        if (!currentUser) {
            setStatus("Sign in before uploading.");
            return;
        }

        if (!selectedFile) {
            setStatus("Choose a file before uploading.");
            return;
        }

        try {
            await uploadFile(currentUser.id, selectedFile);
            setSelectedFile(null);
            setStatus("File uploaded and metadata saved.");
            await loadFiles(currentUser.id);
        } catch (error) {
            setStatus(error instanceof Error ? error.message : "There was an error uploading the file.");
        }
    };

    const handleRefreshFiles = async (): Promise<void> => {
        if (!currentUser) {
            return;
        }

        try {
            await loadFiles(currentUser.id);
            setStatus("Files refreshed.");
        } catch (error) {
            setStatus(error instanceof Error ? error.message : "There was an error loading files.");
        }
    };

    return (
        <main className="app-shell">
            <section className="panel">
                <h1>S3 Bucket Test</h1>
                <p>Sign in with your name and email to upload files and view saved S3 metadata.</p>
            </section>

            {!currentUser ? (
                <section className="panel">
                    <h2>Account</h2>
                    <div className="form-grid">
                        <label>
                            Name
                            <input value={name} onChange={(event) => setName(event.target.value)} />
                        </label>
                        <label>
                            Email
                            <input value={email} onChange={(event) => setEmail(event.target.value)} />
                        </label>
                    </div>
                    <div className="button-row">
                        <button onClick={handleLogin}>Sign In</button>
                        <button onClick={handleCreateUser}>Create Account</button>
                    </div>
                    {status && <p>{status}</p>}
                </section>
            ) : (
                <>
                    <section className="panel panel-header">
                        <div>
                            <h2>{currentUser.name}</h2>
                            <p>{currentUser.email}</p>
                        </div>
                        <button onClick={handleSignOut}>Sign Out</button>
                    </section>

                    <section className="panel">
                        <h2>Upload File</h2>
                        <input type="file" accept={acceptedFilesTypes.join(",")} onChange={handleFile} />
                        <div className="button-row">
                            <button onClick={handleUpload}>Upload</button>
                            <button onClick={handleRefreshFiles}>Refresh Files</button>
                        </div>
                        {status && <p>{status}</p>}
                    </section>

                    <section className="panel">
                        <h2>User Files</h2>
                        {files.length === 0 ? (
                            <p>No files uploaded yet.</p>
                        ) : (
                            <ul className="file-list">
                                {files.map((file: S3Item) => (
                                    <li key={file.id}>
                                        <strong>{file.fileName}</strong>
                                        <span>{file.mimeType}</span>
                                        <code>{file.s3Key}</code>
                                        <a href={file.fileUrl} target="_blank" rel="noreferrer">
                                            Open File
                                        </a>
                                    </li>
                                ))}
                            </ul>
                        )}
                    </section>
                </>
            )}
        </main>
    );
}
