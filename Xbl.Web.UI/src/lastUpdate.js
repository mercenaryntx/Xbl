// src/lastUpdate.js
export const LAST_UPDATE_KEY = 'X-Titles-Last-Update';

export function setLastUpdate(value) {
    if (value) {
        localStorage.setItem(LAST_UPDATE_KEY, value);
    }
}

export async function getLastUpdate(apiBaseUrl) {
    let lastUpdate = localStorage.getItem(LAST_UPDATE_KEY);
    if (lastUpdate) return lastUpdate;
    const response = await fetch(`${apiBaseUrl}/Titles`, { method: 'OPTIONS' });
    lastUpdate = response.headers.get(LAST_UPDATE_KEY);
    setLastUpdate(lastUpdate);
    return lastUpdate;
}

export async function getHeaders(apiBaseUrl) {
	const headers = {
		'X-Titles-Last-Update': await getLastUpdate(apiBaseUrl)
	};
	return headers;
}