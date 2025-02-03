async function print_val(val){
    console.log(val);
}

async function search_for_email( accountList ){
    return accountList.filter(account => account.name == emailAccountName)[0].id
}

async function search_for_inbox( accountId ){
    return messenger.folders.query({"accountId":accountId,"name":"Inbox"});
}

async function return_singleton_element( array ){
    return array[0];
}

async function* getMessages(list) {
  let page = await list;
  for (let message of page.messages) {
    yield message;
  }

  while (page.id) {
    page = await messenger.messages.continueList(page.id);
    for (let message of page.messages) {
      yield message;
    }
  }
}

async function parse_folder( folder ){
    let message_list = messenger.messages.query({"folderId":folder.id,"unread":true});
    let batch_iterator = await getMessages(message_list);
    let batch = [];
    let batch_count = 0;
    for await (let batch_item of batch_iterator ){
        batch.push(batch_item);
        batch_count += 1;
    }
    let eidx = batch.length - 1;
    let con_batch = [];
    for (let i = 0; i < 5; i++){
        con_batch.push(batch[eidx - i]);
    }
    return [batch_count, con_batch];;
    // return messenger.messages.list(folder);
}


async function getUnread(){
    return messenger.accounts.list().then( search_for_email )
                             .then( search_for_inbox )
                             .then( return_singleton_element )
                             .then( parse_folder )
                             // .then(print_val);
}

async function sendToNativeApp(data){
    let port = browser.runtime.connectNative('thunderbird_live_tile');
    port.postMessage(data);
}

async function getUnreadWrapper(){
  getUnread().then( sendToNativeApp ).then(console.log('exec'))
}
// Initialize after Thunderbird APIs are ready
document.addEventListener("DOMContentLoaded", () => {
  console.log("Extension Loaded");
  getUnread().then( sendToNativeApp );
  setInterval(getUnreadWrapper, 3000);
});

