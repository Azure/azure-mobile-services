// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\base.js" />
/// <reference path="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.WinJS.1.0\1.0\DesignTime\CommonConfiguration\Neutral\Microsoft.WinJS.1.0\js\ui.js" />
/// <reference path="..\..\js\MobileServices.Internals.js" />
/// <reference path="..\..\generated\Tests.js" />

$testGroup('Blogging')
    .functional()
    .tests(
        $test('UseBlog')
        .description('Create and manipulate posts/comments')
        .checkAsync(function () {
            var client = $getClient();
            var postTable = client.getTable('blog_posts');
            var commentTable = client.getTable('blog_comments');
            var context = {};
            return $chain(
                function () {
                    $log('Add a few posts and a comment');
                    return postTable.insert({ title: "Windows 8" });
                },
                function (post) {
                    context.post = post;
                    context.newItems = 'id ge ' + post.id;
                    return postTable.insert({ title: "ZUMO" });
                },
                function (highlight) {
                    context.highlight = highlight;
                    return commentTable.insert({
                        postid: context.post.id,
                        name: "Anonymous",
                        commentText: "Beta runs great"
                    });
                },
                function () {
                    return commentTable.insert({
                        postid: context.highlight.id,
                        name: "Anonymous",
                        commentText: "Whooooo"
                    });
                },
                function () {
                    return postTable.where(context.newItems).read();
                },
                function (items) {
                    $assert.areEqual(2, items.length);

                    $log('Add another comment to the first post');
                    return commentTable.insert({ postid: context.post.id, commentText: "Can't wait" });
                },
                function (opinion) {
                    $assert.areNotEqual(0, opinion.id);
                });
        })
    );
