import { getTokenWorkaround } from "@/app/actions/authActions";

const baseUrl = "http://localhost:6001/";

async function get(url: string) {
  const requestOptions = {
    method: "GET",
    headers: await getHeaders(),    
  };

  const response = await fetch(`${baseUrl}${url}`, requestOptions);
  
  return await handleResponse(response);  
}

async function post(url: string, body: {}) {
  const requestOptions = {
    method: "POST",
    headers: await getHeaders(),   
    body: JSON.stringify(body),
  };  

  const response = await fetch(`${baseUrl}${url}`, requestOptions);

  return await handleResponse(response);  
}

async function put(url: string, id: string, body: {}) {
  const requestOptions = {
    method: "PUT",
    headers: await getHeaders(),  
    body: JSON.stringify(body),
  };  

  const response = await fetch(`${baseUrl}${url}/${id}`, requestOptions);

  return await handleResponse(response);  
}

async function del(url: string, id: string) {
  const requestOptions = {
    method: "DELETE",
    headers: await getHeaders()
  };  

  const response = await fetch(`${baseUrl}${url}/${id}`, requestOptions);

  return await handleResponse(response);  
}

async function handleResponse(response: Response) {
  const text = await response.text();
  const data = parseText(text);

  if(!response.ok) {
    const error = {
      status: response.status,
      message: typeof data === "string" ? data : response.statusText
    };
    return {error};
  }  

  return data || response.statusText;
}

function parseText(text: string) {
  try {
    return JSON.parse(text);
  } catch(e) {
    return text;
  }  
}

async function getHeaders() {
  const token = await getTokenWorkaround();
  const headers = { "Content-Type": "application/json" } as any;
  if(token) {
    headers["Authorization"] = `Bearer ${token?.access_token}`;
  }
  return headers;
}

export const fetchWrapper = {
  get,
  post,
  put,
  del
};

