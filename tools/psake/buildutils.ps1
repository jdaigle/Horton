function Delete-Directory($directoryName){
    Remove-Item -Force -Recurse $directoryName -ErrorAction SilentlyContinue
}
 
function Create-Directory($directoryName){
    New-Item $directoryName -ItemType Directory -Force | Out-Null
}