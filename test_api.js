process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

async function testApi() {
    try {
        const tokenRes = await fetch("https://localhost:44386/connect/token", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: new URLSearchParams({
                grant_type: "password",
                username: "adminuser",
                password: "Dev@123456",
                client_id: "Alumni_Swagger"
            }),
        });

        if (!tokenRes.ok) {
            console.error("Token failed:", tokenRes.status, await tokenRes.text());
            return;
        }

        const tokenData = await tokenRes.json();
        const token = tokenData.access_token;
        console.log("Got Token");

        // Fetch requests
        const reqRes = await fetch("https://localhost:44386/api/app/certificate-request?skipCount=0&maxResultCount=10", {
            headers: {
                "Authorization": `Bearer ${token}`,
                "Accept": "application/json"
            }
        });

        if (!reqRes.ok) {
            console.error("API failed:", reqRes.status, await reqRes.text());
            return;
        }

        const resData = await reqRes.json();
        console.log("Success! Total count:", resData.totalCount);
        console.log("Items:", JSON.stringify(resData.items, null, 2));

    } catch (err) {
        console.error("Error:", err);
    }
}

testApi();
