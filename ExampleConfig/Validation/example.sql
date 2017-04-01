-- Context: The following records have invalid AG_STATE codes
select * 
from ArsonAgencyReferral A 
left join States S 
on A.AG_STATE = S.strStateCode 
where S.strStateCode is NULL;