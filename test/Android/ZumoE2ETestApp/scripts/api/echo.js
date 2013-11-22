exports.post = function(request, response) {
	response.send(200, JSON.stringify(request.body));
};