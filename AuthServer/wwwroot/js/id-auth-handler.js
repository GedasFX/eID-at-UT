import * as webeid from './web-eid.js';

const lang = navigator.language.substring(0, 2);
const authButton = document.querySelector("#webeid-auth-button");
const errorBox = document.querySelector("#webeid-error-box");

async function run() {
  try {
    errorBox.style.visibility = 'hidden';

    const challengeResponse = await fetch("/signin-web-eid/challenge", {
      method: "GET",
      headers: {
        "Content-Type": "application/json"
      }
    })

    if (!challengeResponse.ok)
      throw new Error("GET /auth/challenge server error: " + challengeResponse.status);

    const {nonce} = await challengeResponse.json();

    const authToken = await webeid.authenticate(nonce, {lang});

    const authTokenResponse = await fetch("/signin-web-eid/login", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "RequestVerificationToken": document.getElementById("csrfToken").value
      },
      body: JSON.stringify(authToken)
    });

    if (!authTokenResponse.ok) {
      throw new Error("POST /auth/login server error: " + authTokenResponse.status);
    }

    const params = new Proxy(new URLSearchParams(window.location.search), {
      get: (searchParams, prop) => searchParams.get(prop),
    });

    window.location.href = "/signin-web-eid";
  } catch (e) {
    errorBox.style.visibility = 'visible';
    errorBox.innerHTML = e.message;

    throw e;
  }
}

authButton.addEventListener("click", run);
window.addEventListener("load", run);

