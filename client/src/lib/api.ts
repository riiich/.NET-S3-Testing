export const API_BASE_URL = "http://localhost:5291";

export interface User {
	id: number;
	name: string;
	email: string;
	createdAt: string;
}

export interface S3Item {
	id: number;
	userId: number;
	uploadedAt: string;
	fileName: string;
	mimeType: string;
	fileSize: number;
	fileUrl: string;
}

export interface UploadResponse {
	msg: string;
	res: string;
	item: S3Item;
}

export async function createUser(name: string, email: string): Promise<User> {
	const response: Response = await fetch(`${API_BASE_URL}/api/users`, {
		method: "POST",
		headers: {
			"Content-Type": "application/json",
		},
		body: JSON.stringify({ name, email }),
	});

	if (!response.ok) {
		throw new Error(await response.text());
	}

	return (await response.json()) as User;
}

export async function loginUser(name: string, email: string): Promise<User> {
	const response: Response = await fetch(`${API_BASE_URL}/api/users/login`, {
		method: "POST",
		headers: {
			"Content-Type": "application/json",
		},
		body: JSON.stringify({ name, email }),
	});

	if (!response.ok) {
		throw new Error(await response.text());
	}

	return (await response.json()) as User;
}

export async function getUser(userId: number): Promise<User> {
	const response: Response = await fetch(`${API_BASE_URL}/api/users/${userId}`);

	if (!response.ok) {
		throw new Error("Account was not found.");
	}

	return (await response.json()) as User;
}

export async function uploadFile(userId: number, file: File): Promise<UploadResponse> {
	const formData: FormData = new FormData();
	formData.append("uploadedFile", file);
	formData.append("userId", userId.toString());

	const response: Response = await fetch(`${API_BASE_URL}/api/s3/upload`, {
		method: "POST",
		body: formData,
	});

	if (!response.ok) {
		throw new Error(await response.text());
	}

	return (await response.json()) as UploadResponse;
}

export async function getUserFiles(userId: number): Promise<S3Item[]> {
	const response: Response = await fetch(`${API_BASE_URL}/api/s3-items/user/${userId}`);

	if (!response.ok) {
		throw new Error(await response.text());
	}

	return (await response.json()) as S3Item[];
}

export async function deleteFile(s3ItemId: number, userId: number): Promise<void> {
	const response: Response = await fetch(`${API_BASE_URL}/api/s3-items/users/${userId}/${s3ItemId}`, {
		method: "DELETE",
	});

    if (!response.ok) {
		throw new Error(await response.text());
	}
}
