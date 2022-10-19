angular.module("umbraco").controller("CustomWelcomeDashboardController", function ($scope, userService, logResource, entityResource){
    var vm = this;
    vm.UserName = "guest";
    vm.UserLogHistory = [];
    var user = userService.getCurrentUser().then(function(user) {
        console.log(user);
        vm.UserName = user.name;
    });
    var userLogOptions = {
        pageSize: 10,
        pageNumber: 1,
        orderDirection: "Descending",
        sinceDate: new Date(2018, 0, 1)
    };

    logResource.getPagedUserLog(userLogOptions)
    .then(function (response) {
        console.log(response);
        vm.UserLogHistory = response;
        var filteredLogEntries = [];
        // Define supported entity types
        var supportedEntityTypes = ["Document", "Media", "MemberType", "MemberGroup", "MediaType", "DocumentType", "Member", "DataType"];
        // Loop through the response, and flter out save log entries we are not interested in
        angular.forEach(response.items, function (item) {
            // if the query finds a log entry for an entity type that isn't supported, nothing should done with that
            if (!supportedEntityTypes.includes(item.entityType)) 
                {
                    return;
                }
            // if no entity exists -1 is returned for the nodeId (eg saving a macro would create a log entry without a nodeid)
            if (item.nodeId > 0) {
                // check if we already grabbed this from the entityservice
                var nodesWeKnowAbout = [];
                if (nodesWeKnowAbout.indexOf(item.nodeId) !== -1)
                    return;
                // find things the user saved and/or published
                if (item.logType === "Save" || item.logType === "SaveVariant" || item.logType === "Publish") {
                    // check if it is media or content
                    if (item.entityType === "Document") {
                        item.editUrl = "content/content/edit/" + item.nodeId;
                    }
                    if (item.entityType === "Media") {
                        item.editUrl = "media/media/edit/" + item.nodeId;
                    }

                    if (typeof item.entityType !== 'undefined') {
                        // use entityResource to retrieve details of the content/media item
                        var ent = entityResource.getById(item.nodeId, item.entityType).then(function (ent) {
                            console.log(ent);
                            item.Content = ent;
                        });

                        nodesWeKnowAbout.push(ent.id);
                        filteredLogEntries.push(item);
                    }
                }
            }
        });
        vm.UserLogHistory.items = filteredLogEntries;
    });
});
