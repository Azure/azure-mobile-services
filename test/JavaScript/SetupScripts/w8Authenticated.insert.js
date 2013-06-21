function insert(item, user, request) {
    item.userId = user.userId;
    request.execute();
}
