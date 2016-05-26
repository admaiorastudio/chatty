/**
 * My API Sandbox
 * 
 */
 
 // Users list
 state.users = state.users || [];
 // Messages list
 state.messages = state.messages || [];

Sandbox.define('/users/login','POST', function(req, res) {
    
    // Set the type of response, sets the content type.
    res.type('application/json');
    
    // Check the request, make sure it is a compatible type
    if (!req.is('application/json')) {
        return res.send(400, { status: "error", message: "Invalid content type, expected application/json" });
    }
    
    if(req.body === null) {
        return res.send(400, { status: "error", message: "Invalid content type, expected application/json" });
    }
    
    var username = req.body.username;
    if(username === undefined || username.length == 0) {
        return res.json(400, { status: "error", message: "Missing username!" });
    }
    
    var password = req.body.password;
    if(password === undefined || password.length == 0) {
        return res.json(400, { status: "error", message: "Missing password!" });
    }
    
    state.users = state.users || [];
    if(_.find(state.users, { "username": username })) {
        username = username + Math.random().toString(5).replace(/[^a-z]+/g, '');
    }
    
    state.users.push({ "username": username });
    
    // Set the status code of the response.
    res.status(200);
    
    // Send the response body.
    res.json({
        "status": "ok",
        "username": username,
        "token": Math.random().toString(36).replace(/[^a-z]+/g, '')
    });
})

Sandbox.define('/users/logout','POST', function(req, res) {
    
    // Set the type of response, sets the content type.
    res.type('application/json');
    
    var token = req.get('Access-Token');
    if(token === undefined || token == "undefined" || token.length == 0) {
        return res.json(401, { status: "error", message: "Unauthorized access. Invalid token!" });
    }    
    
    // Check the request, make sure it is a compatible type
    if (!req.is('application/json')) {
        return res.send(400, { status: "error", message: "Invalid content type, expected application/json" });
    }
    
    if(req.body === null) {
        return res.send(400, { status: "error", message: "Invalid content type, expected application/json" });
    }
    
    var username = req.body.username;
    if(username === undefined || username.length == 0) {
        return res.json(400, { status: "error", message: "Missing username!" });
    }
    
    if(_.find(state.users, { "username": username }) === undefined) {
        return res.json(401, { status: "error", message: "Invalid user. please login!" })
    }
    
    _.remove(state.users, function(u) { return u.username === username });
    
    // Set the status code of the response.
    res.status(200);
    
    // Send the response body.
    res.json({
        "status": "ok"
    });
})

Sandbox.define('/users/all','GET', function(req, res) {

    // Set the type of response, sets the content type.
    res.type('application/json');
    
    var token = req.get('Access-Token');
    if(token === undefined || token == "undefined" || token.length == 0) {
        return res.json(401, { status: "error", message: "Unauthorized access. Invalid token!" });
    }    
    
    // Set the status code of the response.
    res.status(200);
    
    // Send the response body.
    res.json({
        "status": "ok",
        "users": state.users
    });    

})

Sandbox.define('/messages/all','GET', function(req, res) {

    // Set the type of response, sets the content type.
    res.type('application/json');
    
    var token = req.get('Access-Token');
    if(token === undefined || token == "undefined" || token.length == 0) {
        return res.json(401, { status: "error", message: "Unauthorized access. Invalid token!" });
    }    
    
    // Set the status code of the response.
    res.status(200);
    
    // Send the response body.
    res.json({
        "status": "ok",
        "messages": state.messages
    });    

})

Sandbox.define('/messages/send','POST', function(req, res) {
    
    // Set the type of response, sets the content type.
    res.type('application/json');
    
    var token = req.get('Access-Token');
    if(token === undefined || token == "undefined" || token.length == 0) {
        return res.json(401, { status: "error", message: "Unauthorized access. Invalid token!" });
    }    
    
    // Check the request, make sure it is a compatible type
    if (!req.is('application/json')) {
        return res.send(400, { status: "error", message: "Invalid content type, expected application/json" });
    }
    
    if(req.body === null) {
        return res.send(400, { status: "error", message: "Invalid content type, expected application/json" });
    }
    
    var username = req.body.username;
    if(username === undefined || username.length == 0) {
        return res.json(400, { status: "error", message: "Missing username!" });
    }
    
    var content = req.body.content;
    if(content === undefined || content.length == 0) {
        return res.json(400, { status: "error", message: "Missing password!" });
    }
    
    if(_.find(state.users, { "username": username }) === undefined) {
        return res.json(400, { status: "error", message: "Invalid user. please login!" });
    }
    
    state.messages = state.messages || [];
    state.messages.push({ "username": username, "content": content });
    
    // Set the status code of the response.
    res.status(200);
    
    // Send the response body.
    res.json({
        "status": "ok",
        "messages": state.messages
    });
})