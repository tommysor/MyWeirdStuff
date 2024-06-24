echo '{"microsoft.net.sdk.aspire": "8.0.1/8.0.100"}' > ./aspireVersion55168915364.txt
dotnet workload install aspire --from-rollback-file ./aspireVersion55168915364.txt
rm ./aspireVersion55168915364.txt
