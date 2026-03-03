export async function request(url, options = {}) {
    const method = options.method || "GET";
    const body = options.body;
    const isFormData = options.isFormData || body instanceof FormData;

    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        xhr.open(method, url, true);

        if (!isFormData) {
            xhr.setRequestHeader("Content-Type", "application/json");
        }

        if (options.onProgress && xhr.upload) {
            xhr.upload.onprogress = options.onProgress;
        }

        xhr.onload = () => {
            let result;
            try {
                result = typeof xhr.responseText === 'string' && xhr.responseText.length > 0
                    ? JSON.parse(xhr.responseText)
                    : {};
            } catch (e) {
                result = xhr.responseText;
            }

            if (xhr.status >= 200 && xhr.status < 300) {
                resolve(result);
            } else {
                reject(new Error(result.message || result || "Network Error"));
            }
        };

        xhr.onerror = () => reject(new Error("Network connection failed"));

        if (body) {
            xhr.send(isFormData ? body : JSON.stringify(body));
        } else {
            xhr.send();
        }
    });
}

export function addEventIfElementExists(selector, eventType, callback) {
    const element = document.querySelector(selector);
    if (element) {
        element.addEventListener(eventType, callback);
    }
}
