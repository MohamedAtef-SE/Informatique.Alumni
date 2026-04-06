$path = "d:\Projects\Informatique.Alumni\client\src\pages\admin\AlumniManager.tsx"
$content = [System.IO.File]::ReadAllText($path)

# Regex to match the entire cell content from <TableCell to </TableCell>
$advisorCellPattern = '(?s)<TableCell className="text-center">\s*<button\s*onClick=\{[^}]+\}\s*className=\{`p-1\.5 rounded-full transition-colors cursor-pointer \${item\.isAdvisor[^`]+`\}\s*title=\{item\.isAdvisor \? "Demote from Advisor" : "Promote to Advisor"\}\s*>\s*<ShieldCheck className=\{`w-5 h-5 \${item\.isAdvisor \? \''fill-current\'' : \'\''\}`\} />\s*</button>\s*</TableCell>'

$newCellContent = @'
                                            <TableCell className="text-center">
                                                {item.advisoryStatus === AdvisoryStatus.Requested ? (
                                                    <Button 
                                                        size="sm" 
                                                        variant="outline" 
                                                        className="h-8 border-indigo-500/50 text-indigo-400 hover:bg-indigo-500/10"
                                                        onClick={() => openReview(item.id)}
                                                    >
                                                        Review
                                                    </Button>
                                                ) : (
                                                    <button 
                                                        onClick={() => toggleAdvisorMutation.mutate(item.id)}
                                                        className={`p-1.5 rounded-full transition-colors cursor-pointer ${item.advisoryStatus === AdvisoryStatus.Approved ? 'text-indigo-400 bg-indigo-400/10' : 'text-slate-600 hover:text-slate-400 hover:bg-slate-700/30'}`}
                                                        title={item.advisoryStatus === AdvisoryStatus.Approved ? "Demote from Advisor" : "Promote to Advisor"}
                                                    >
                                                        <ShieldCheck className={`w-5 h-5 ${item.advisoryStatus === AdvisoryStatus.Approved ? 'fill-current' : ''}`} />
                                                    </button>
                                                )}
                                            </TableCell>
'@

# Replace Using regex
if ($content -match $advisorCellPattern) {
    Write-Host "Found Advisor Cell via Regex!"
    $content = [regex]::Replace($content, $advisorCellPattern, $newCellContent)
} else {
    Write-Warning "Advisor Cell NOT found via Regex."
}

# 2. Add Modal at the end (before the last closing brace/div)
# Using a simpler anchor: just before "</div>\s*);\s*};" at the end of the file
$endPattern = '(?s)(</div>\s*);\s*};\s*export default AlumniManager;'
$newEndContent = @"
            <AdvisorReviewModal 
                alumni={reviewAlumni}
                isOpen={!!reviewAlumni}
                onClose={() => setReviewAlumni(null)}
                onApprove={(id) => approveAdvisorMutation.mutate(id)}
                onReject={(id, reason) => rejectAdvisorMutation.mutate({ id, reason })}
            />
        `$1
    };

export default AlumniManager;
"@

if ($content -match $endPattern) {
    Write-Host "Found End Pattern!"
    $content = [regex]::Replace($content, $endPattern, $newEndContent)
}

[System.IO.File]::WriteAllText($path, $content)
