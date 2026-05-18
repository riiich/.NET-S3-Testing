import { useState } from "react";

export default function FileUploader() {
	const file = new FormData();
	const acceptedFilesTypes = [
		"application/pdf",
		"application/rtf",
		"text/plain",
		"image/png",
		"image/jpeg",
		"image/gif",
	];

	const handleFile = (e: React.ChangeEvent<HTMLInputElement>) => {
		const currFile = e.target.files?.[0];

		if (!currFile) {
			console.log("boof ass file");
			return;
		}

		file.append("uploadedFile", currFile);	// name MUST match the variable name in the backend 
	};

	const fileUpload = async () => {
		// api call to send file to server
		const res = await fetch("http://localhost:5291/api/s3/upload", {
			method: "POST",
			body: file,
		});

		const data = await res.json();

		console.log("data:", data);
	};

	return (
		<div>
			<section>FileUploader</section>

			<section>
				<input type="file" accept={acceptedFilesTypes.join(",")} onChange={handleFile} />
				<button onClick={fileUpload}>Upload</button>
			</section>
		</div>
	);
}
